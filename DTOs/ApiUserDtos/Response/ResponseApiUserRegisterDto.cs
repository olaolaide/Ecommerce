using Ecommerce.Models.AuthModels;

namespace Ecommerce.DTOs.ApiUserDtos.Response;

public class ResponseApiUserRegisterDto
{
    public bool IsSuccessful { set; get; }
    public ApiUser ApiUser { set; get; }
    public List<string> Message { set; get; } = [];
}