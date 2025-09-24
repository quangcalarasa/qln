using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Dto
{
    public class LoginModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class ChangePassUserModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class LoginSsoModel
    {
        public string username { get; set; }
        public string token { get; set; }
    }
}
