using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicPlayerBackend.Common;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects.User;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[Route("[controller]/[action]")]
public sealed class UserController(ILogger<UserController> logger, UserManager<User> userManager, SignInManager<User> signInManager, IUserResolver userResolver, IOptions<AppConfig> appConfig) : ControllerBase
{
    private readonly AppConfig _appConfig = appConfig.Value;

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

    [HttpPost]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user == default)
            return BadRequest(new { Error = "Can't find user or wrong password" });

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (!signInResult.Succeeded)
            return BadRequest(new { Error = signInResult });

        var claims = new[] {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_appConfig.JwtSecret));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(228),
            signingCredentials: signingCredentials
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse { JwtBearer = jwt });
    }
}
