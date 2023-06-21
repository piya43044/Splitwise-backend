using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace EMS.Users
{
    public class FindTheUserNameAppService : ApplicationService
    {
        private readonly IIdentityUserRepository _identityUserRepository;

        public FindTheUserNameAppService(IIdentityUserRepository identityUserRepository)
        {
            _identityUserRepository = identityUserRepository;
        }

        public async Task<List<UserDto>> GetUserList()
        {
            var userList = await _identityUserRepository.GetListAsync();

            var userDtoList = userList.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            }).ToList();

            return userDtoList;
        }
        public async Task<UserDto> GetUserById(Guid userId)
        {
            var userDto = await _identityUserRepository.GetAsync(userId);
            return MapToUserDto(userDto);
        }

        private UserDto MapToUserDto(IdentityUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }


    }
}