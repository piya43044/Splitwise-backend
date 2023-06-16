using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using static Volo.Abp.Identity.IdentityPermissions;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using static Volo.Abp.UI.Navigation.DefaultMenuNames.Application;

namespace EMS.Friends
{
    public class AddFriendAppService : ApplicationService, IAddFriendAppService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<Friend, Guid> _friendRepository;

        public AddFriendAppService(UserManager<IdentityUser> userManager, IGuidGenerator guidGenerator, ICurrentUser currentUser, IRepository<Friend, Guid> friendRepository)
        {
            _userManager = userManager;
            _guidGenerator = guidGenerator;
            _currentUser = currentUser;
            _friendRepository = friendRepository;
        }

        public async Task<ResponseModel> AddFriendAsync(string name, string userMail)
        {
            var emailId = userMail;
            var existingUser = await _userManager.FindByEmailAsync(emailId);
            if (!Regex.IsMatch(emailId, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new BusinessException("Invalid email format");
            }
            if (existingUser != null)
            {
                //check if friendship of existingUser and current user exist

                // throw new BusinessException("User already exists");
                var existingFriendship = await _friendRepository.FirstOrDefaultAsync(f =>
                f.UserId == new Guid("3a0bc559-4a98-1dfa-317d-c94539d43a69") && f.FriendId == existingUser.Id && !f.IsDeleted);

                if (existingFriendship != null)
                {
                    return new ResponseModel { result = "Friend exist" };
                }

                var friend = new Friend
                {
                    UserId = new Guid("3a0bc559-4a98-1dfa-317d-c94539d43a69"),
                    FriendId = existingUser.Id,
                    IsDeleted = false
                };

                await _friendRepository.InsertAsync(friend);


                return new ResponseModel { result = "Friend added" };
            }
            else
            {
                await SendEmailAsync(emailId);
                var user = new IdentityUser(_guidGenerator.Create(), name, emailId);
                var resultUser = await _userManager.CreateAsync(user);

                if (resultUser.Succeeded)
                {
                    //var currentUser = await _currentUser.Id();
                    var friend = new Friend
                    {
                        UserId = new Guid("3a0bc559-4a98-1dfa-317d-c94539d43a69"),
                    FriendId = user.Id,
                        IsDeleted = false
                    };
                    await _friendRepository.InsertAsync(friend);
                  
                    return new ResponseModel { result = "Friend invited" };
                }
                
                throw new ApplicationException($"Could not add user: {resultUser.Errors.FirstOrDefault()?.Description}");
            }

        }

        private async Task SendEmailAsync(string targetEmail)
        {
            var mailMessage = new MailMessage();
            mailMessage.To.Add(targetEmail);
            mailMessage.From = new MailAddress("test1403email@gmail.com");
            mailMessage.Subject = "Join Splitwise";
            mailMessage.Body = "Please join to Splitwise using this link https://secure.splitwise.com/login";

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential("test1403email@gmail.com", "honxqgcfvhfksxfo");
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
