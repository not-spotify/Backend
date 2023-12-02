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
    IUserResolver userResolver,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<TokenConfig> tokenConfig) : ControllerBase
{
    private readonly TokenConfig _tokenConfig = tokenConfig.Value;

    /// <summary>
    /// Gets authorized User.
    /// </summary>
    /// <returns>Authorized User</returns>
    /// <response code="200">Returns User</response>
    /// <response code="400">Wrong schema</response>
    /// <response code="401">Not authorized</response>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var user = await userResolver.GetUserAsync();
        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates User.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Jwt Bearer</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /User/Register
    ///     {
    ///       "userName": "metauser",
    ///       "email": "meta@mail.local",
    ///       "password": "somesecurepassword"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns User identifier</response>
    /// <response code="400">Wrong schema</response>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            logger.LogWarning("User {user} tried to register again", await userResolver.GetUserIdAsync());
            return BadRequest("You're already registered");
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            HashedPassword = request.Password
        };

        var result = await userManager.CreateAsync(user);

        if (result != IdentityResult.Success)
            return BadRequest(new { result.Errors });

        return Ok(new RegisterResponse { Id = user.Id });
    }

    /// <summary>
    /// Get Jwt Bearer for using secure actions.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Jwt Bearer</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /User/Login
    ///     {
    ///       "email": "meta@mail.local",
    ///       "password": "somesecurepassword"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns JWT Bearer</response>
    /// <response code="401">Wrong email or password</response>
    [HttpPost]
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

    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        var existingRefreshToken = await refreshTokenRepository.GetValidTokenOrDefault(request.UserId, request.Jti, Guid.Parse(request.RefreshToken));
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
