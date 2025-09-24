using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class UserLoginData : User
    {
        public string AccessKey { get; set; }
        public string AccessToken { get; set; }
        public string RoleCode { get; set; }
        public string BaseUrlImg { get; set; }
        public string BaseUrlFile { get; set; }
        public List<MenuData> listMenus { get; set; }
    }

    public class UserData : User
    {
        public List<UserRole> userRoles { get; set; }
        public List<WardManagement> wardManagements { get; set; }
    }

    public class UserDataExport : User 
    { 
        public string Role { get; set; }
    }
}
