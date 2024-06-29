using System.ComponentModel.DataAnnotations;

namespace Ecommerce.DTOs.ApiUserDtos.Request;

public class RequestApiUserRegisterDto
{
    [Required] [EmailAddress] public string Email { get; set; }

    [Required]
    [StringLength(25, ErrorMessage = "Your password is limited to 8 and 25 character", MinimumLength = 8)]
    public string Password { get; set; }

    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
}