using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167ManagePurpose : AbstractEntity<int>
    {
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
