using Ecommerce.DTOs.ApiUserDtos.Request;
using Ecommerce.DTOs.ApiUserDtos.Response;
using Ecommerce.Models.AuthModels;
using Ecommerce.Repositories.AuthRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Ecommerce.Templates;

namespace Ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    IAuthRepository authRepository,
    UserManager<ApiUser> userManager,
    IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RequestApiUserRegisterDto userRegisterDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var registrationResult = await authRepository.Register(userRegisterDto);
        if (!registrationResult.IsSuccessful)
        {
            return BadRequest(new ResponseApiUserRegisterDto
            {
                IsSuccessful = false,
                Message = registrationResult.Message
            });
        }

        var user = registrationResult.ApiUser;
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            $"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, code = token })}";
        var emailSender = new EmailSender(configuration["MailJetApiKey"], configuration["MailJetSecretKey"]);
        await emailSender.SendEmailAsync(userRegisterDto.Email,
            $"{userRegisterDto.FirstName} {userRegisterDto.LastName}", confirmationLink);

        return Ok(new ResponseApiUserRegisterDto
        {
            IsSuccessful = true,
            ApiUser = user
        });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new ResponseApiUserConfirmEmail()
            {
                IsSuccessful = false,
                Message = "Wrong email confirmation link"
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded)
        {
            return Ok("Email confirmed successfully.");
        }

        return BadRequest("Error confirming email.");
    }


    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RequestApiUserRegisterDto userDto,
        [FromQuery] int secretKey)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await authRepository.RegisterAdmin(userDto, secretKey);
        if (result.IsSuccessful)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResponseLoginDto>> Login([FromBody] RequestLoginDto userLogin)
    {
        var authResponse = await authRepository.Login(userLogin);
        if (authResponse.Result == false)
        {
            return BadRequest(authResponse);
        }

        return Ok(authResponse);
    }
}