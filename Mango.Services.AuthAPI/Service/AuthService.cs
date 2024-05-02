using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly RoleManager<IdentityRole> _rolemanager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> rolemanager)
        {
            _db = db;
            _userManager = userManager;
            _rolemanager = rolemanager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string rolename)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_rolemanager.RoleExistsAsync(rolename).GetAwaiter().GetResult())
                {
                    _rolemanager.CreateAsync(new IdentityRole(rolename)).GetAwaiter().GetResult();
                }

                await _userManager.AddToRoleAsync(user, rolename);
                return true;
            }
            return false;
        }

        public async Task<LoginReponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.Username.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || isValid == false)
            {
                return new LoginReponseDto() { User = null, Token = "" };
            }

            //if user was found , Generate JWT Token
            var role = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, role);

            UserDto userDTO = new()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            LoginReponseDto loginResponseDto = new LoginReponseDto()
            {
                User = userDTO,
                Token = token,
            };

            return loginResponseDto;
        }

        public async Task<string> Regiter(RegistrationRequestDTO registrationRequestDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.Email,
                Email = registrationRequestDTO.Email,
                NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                Name = registrationRequestDTO.Name,
                PhoneNumber = registrationRequestDTO.PhoneNumber,
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if (result.Succeeded) 
                {
                    var userToReturn = _db.ApplicationUsers.First(u=> u.UserName == registrationRequestDTO.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        Id = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber,
                    };
                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }catch (Exception ex)
            {

            }
            return "Error Encountered";
        }
    }
}
