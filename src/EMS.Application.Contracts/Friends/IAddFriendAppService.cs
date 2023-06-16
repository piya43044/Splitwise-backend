using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Http;

namespace EMS.Friends
{
    public interface IAddFriendAppService : IApplicationService
    {
        Task<ResponseModel> AddFriendAsync(string name, string email);
    }
}
