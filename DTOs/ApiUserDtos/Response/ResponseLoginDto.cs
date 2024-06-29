namespace Ecommerce.DTOs.ApiUserDtos.Response;

public class ResponseLoginDto
{
    public string UserId { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool Result { get; set; }
    public List<string> Errors { get; set; } = [];
}