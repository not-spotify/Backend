using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicPlayerBackend.Common;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects;
using MusicPlayerBackend.TransferObjects.User;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[Route("[controller]/[action]")]
public sealed class UserController(ILogger<UserController> logger,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IUserProvider userProvider,
    IRefreshTokenRepository refreshTokenRepository,
    IPlaylistRepository playlistRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<TokenConfig> tokenConfig) : ControllerBase
{
    private readonly TokenConfig _tokenConfig = tokenConfig.Value;

    /// <summary>
    ///     Gets authorized User.
    /// </summary>
    /// <returns>Authorized User</returns>
    /// <response code="200">Returns User</response>
    /// <response code="400">Wrong schema</response>
    /// <response code="401">Not authorized</response>
    [Authorize]
    [HttpGet(Name = "GetMe")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var user = await userProvider.GetUserAsync();
        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };

        return Ok(response);
    }

    /// <summary>
    ///     Creates User.
    /// </summary>
    /// <returns>User identifier and JWT Bearer</returns>
    /// <response code="200">Returns User identifier</response>
    /// <response code="400">Wrong schema</response>
    [HttpPost(Name = "RegisterUser")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            logger.LogWarning("User {user} tried to register again", await userProvider.GetUserIdAsync());
            return BadRequest("You're already registered");
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
        };
        var result = await userManager.CreateAsync(user, password: request.Password);
        if (result != IdentityResult.Success)
            return BadRequest(new { result.Errors });

        await unitOfWork.BeginTransactionAsync();

        userRepository.Save(user);
        await unitOfWork.SaveChangesAsync();

        var playlist = new Playlist
        {
            Visibility = PlaylistVisibility.Private,
            Name = request.UserName + "'s Favorites",
            OwnerUserId = user.Id
        };
        playlistRepository.Save(playlist);
        await unitOfWork.SaveChangesAsync();

        user.FavoritePlaylistId = playlist.Id;
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.CommitAsync();

        return Ok(new RegisterResponse { Id = user.Id });
    }

    /// <summary>
    /// Get JWT Bearer for using secure actions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>JWT Bearer</returns>
    /// <response code="200">Returns JWT Bearer</response>
    /// <response code="401">Wrong email or password</response>
    [HttpPost(Name = "LogInUser")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == default)
            return Unauthorized(new UnauthorizedResponse { Error = "Can't find user or wrong password" });

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (!signInResult.Succeeded)
            return Unauthorized(new UnauthorizedResponse { Error = signInResult.ToString() });

        var jti = Guid.NewGuid();
        var refreshTokenValue = Guid.NewGuid();
        var jwtValidDue = DateTimeOffset.UtcNow.AddDays(1);
        var refreshValidDue = jwtValidDue.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
            }),
            Expires = jwtValidDue.DateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_tokenConfig.SigningKey)), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            ValidDue = refreshValidDue,
            Jti = jti,
            UserId = user.Id,
            Token = refreshTokenValue
        };
        refreshTokenRepository.Save(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            JwtBearer = tokenHandler.WriteToken(token),
            RefreshToken = refreshTokenValue.ToString(),
            RefreshTokenValidDue = refreshValidDue,
            JwtBearerValidDue = jwtValidDue,
            UserId = user.Id
        });
    }

    /// <summary>
    ///     Gets new JWT Bearer by RefreshToken.
    /// </summary>
    [HttpPost(Name = "RefreshUserToken")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var existingRefreshToken = await refreshTokenRepository.GetValidTokenOrDefault(request.UserId, request.Jti, request.RefreshToken);
        if (existingRefreshToken == default)
            return Unauthorized(new UnauthorizedResponse { Error = "Can't refresh Jwt Bearer" });

        existingRefreshToken.Revoked = true;
        refreshTokenRepository.Save(existingRefreshToken);

        var user = await userRepository.GetByIdAsync(request.UserId);

        var jti = Guid.NewGuid();
        var refreshTokenValue = Guid.NewGuid();
        var jwtValidDue = DateTimeOffset.UtcNow.AddDays(1);
        var refreshValidDue = jwtValidDue.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
            }),
            Expires = jwtValidDue.DateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_tokenConfig.SigningKey)), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        var refreshToken = new RefreshToken
        {
            ValidDue = refreshValidDue,
            Jti = jti,
            UserId = user.Id,
            Token = refreshTokenValue
        };
        refreshTokenRepository.Save(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            JwtBearer = tokenHandler.WriteToken(token),
            RefreshToken = refreshTokenValue.ToString(),
            RefreshTokenValidDue = refreshValidDue,
            JwtBearerValidDue = jwtValidDue,
            UserId = user.Id
        });
    }
}
