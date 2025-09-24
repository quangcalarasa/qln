using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class Token : AbstractEntity<Guid>
    {
        public string AccessToken { get; set; }
    }
}
