using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class RoleData : Role
    {
        public List<FunctionRole> functionRoles { get; set; }
    }
}
