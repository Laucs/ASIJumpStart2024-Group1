using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ProfileViewModel
    {
        public ChangePasswordViewModel UpdatePassword { get; set; }
        public ChangeEmailOrUsernameViewModel UpdateEmailOrUserName { get; set; }
        public string UserCode { get; set; }
        public string Email { get; set; }
    }
}
