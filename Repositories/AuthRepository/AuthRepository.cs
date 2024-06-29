using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Constants;
using Ecommerce.Data;
using Ecommerce.DTOs.ApiUserDtos.Request;
using Ecommerce.DTOs.ApiUserDtos.Response;
using Ecommerce.Models.AuthModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Repositories.AuthRepository;

public class AuthRepository(
    UserManager<ApiUser> userManager,
    IConfiguration configuration,
    ApplicationDbContext dbContext)
    : IAuthRepository
{
    public async Task<ResponseApiUserRegisterDto> Register(RequestApiUserRegisterDto userDto)
    {
        var user = new ApiUser
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Email = userDto.Email,
            UserName = userDto.Email,
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, Roles.Customer);
            return new ResponseApiUserRegisterDto
            {
                IsSuccessful = true,
                ApiUser = user
            };
        }

        var errors = result.Errors.Select(error => error.Description).ToList();
        return new ResponseApiUserRegisterDto
        {
            IsSuccessful = false,
            Message = errors
        };
    }

    public async Task<ResponseApiUserRegisterDto> RegisterAdmin(RequestApiUserRegisterDto userDto, int secretKey)
    {
        var expectedSecretKey = configuration.GetValue<int>("AdminSecretKey");
        if (secretKey != expectedSecretKey)
        {
            return new ResponseApiUserRegisterDto
            {
                IsSuccessful = false,
                Message = ["Invalid secret key"]
            };
        }

        var user = new ApiUser
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Email = userDto.Email,
            UserName = userDto.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, Roles.Administrator);
            return new ResponseApiUserRegisterDto
            {
                IsSuccessful = true,
                ApiUser = user
            };
        }

        var errors = result.Errors.Select(error => error.Description).ToList();
        return new ResponseApiUserRegisterDto
        {
            IsSuccessful = false,
            Message = errors
        };
    }

    public async Task<ResponseLoginDto> Login(RequestLoginDto login)
    {
        var user = await userManager.FindByEmailAsync(login.EmailAddress);
        if (user == null || !await userManager.CheckPasswordAsync(user, login.Password))
        {
            return new ResponseLoginDto
            {
                Result = false,
                Errors = ["Wrong login credentials"]
            };
        }

        if (!user.EmailConfirmed)
        {
            return new ResponseLoginDto
            {
                Result = false,
                Errors = ["You need to confirm your email address"]
            };
        }

        // Generate JWT Token
        var token = await GenerateJwtToken(user);

        // Generate and save Refresh Token
        var refreshToken = await GenerateRefreshToken(user, token);

        return new ResponseLoginDto
        {
            Result = true,
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id
        };
    }

    private async Task<string> GenerateJwtToken(ApiUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        var userClaims = await userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("uid", user.Id),
            }
            .Union(userClaims)
            .Union(roleClaims);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToInt32(configuration["JwtSettings:DurationInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshToken(ApiUser user, string token)
    {
        var existingRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == user.Id);

        if (existingRefreshToken != null)
        {
            dbContext.RefreshTokens.Remove(existingRefreshToken);
            await dbContext.SaveChangesAsync();
        }

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(token);
        var refreshToken = new RefreshToken
        {
            JwtId = tokenContent.Id,
            Token = RandomStringGeneration(23),
            AddedDate = DateTime.UtcNow,
            ExpireDate = DateTime.UtcNow.AddMinutes(110),
            UserId = user.Id
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    private string RandomStringGeneration(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}