using System.ComponentModel.DataAnnotations;

namespace Ecommerce.DTOs.ApiUserDtos.Request;

public class RequestLoginDto
{
    [Required] [EmailAddress] public string EmailAddress { get; set; }
    [Required] public string Password { get; set; }
}