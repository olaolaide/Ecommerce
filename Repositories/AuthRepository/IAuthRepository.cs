using Ecommerce.DTOs.ApiUserDtos.Request;
using Ecommerce.DTOs.ApiUserDtos.Response;

namespace Ecommerce.Repositories.AuthRepository;

public interface IAuthRepository
{
    Task<ResponseApiUserRegisterDto> Register(RequestApiUserRegisterDto userDto);
    Task<ResponseApiUserRegisterDto> RegisterAdmin(RequestApiUserRegisterDto userDto, int secretKey);
}