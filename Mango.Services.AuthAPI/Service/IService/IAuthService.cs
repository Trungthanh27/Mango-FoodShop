using Mango.Services.AuthAPI.Models.Dto;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Regiter(RegistrationRequestDTO registrationRequestDTO);
        Task<LoginReponseDto> Login(LoginRequestDto loginRequestDto);

        Task<bool> AssignRole(string email, string rolename);
    }
}
