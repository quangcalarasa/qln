using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167StateOfUse : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
