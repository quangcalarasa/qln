using IOITQln.Common.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class User : AbstractEntity<int>
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime? Dob { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string KeyLock { get; set; }
        public DateTime? LastLogin { get; set; }
        public string RegEmail { get; set; }
        public int? RoleMax { get; set; }
        public byte? RoleLevel { get; set; }
        public string PasswordKeyCloak { get; set; }
        public bool? AddedOnKeyCloak { get; set; }
        public ModuleSystem? ModuleSystem { get; set; }
    }
}
