using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}