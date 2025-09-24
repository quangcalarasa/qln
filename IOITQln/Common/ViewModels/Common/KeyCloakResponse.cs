using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Common.ViewModels.Common
{
    public class KeyCloakResponse
    {
    }

    public class IntrospectionResponse
    {
        public bool active { get; set; }
        public string scope { get; set; }
        public int exp { get; set; }
        public string client_id { get; set; }
        public string sub { get; set; }
        public string preferred_username { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string session_state { get; set; }
    }
}
