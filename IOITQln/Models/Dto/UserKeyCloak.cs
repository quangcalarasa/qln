using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Dto
{
    public class UserKeyCloak
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public bool enabled { get; set; }
        public string username { get; set; }
        public List<Credential> credentials { get; set; }
    }

    public class Credential
    {
        public string type { get; set; }
        public string value { get; set; }
        public bool temporary { get; set; }

    }
}
