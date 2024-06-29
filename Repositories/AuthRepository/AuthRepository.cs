using Ecommerce.Constants;
using Ecommerce.DTOs.ApiUserDtos.Request;
using Ecommerce.DTOs.ApiUserDtos.Response;
using Ecommerce.Models.AuthModels;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Repositories.AuthRepository;

public class AuthRepository(UserManager<ApiUser> userManager, IConfiguration configuration)
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
}