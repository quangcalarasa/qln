using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class UserDevice : AbstractEntity<long>
    {
        public int UserId { get; set; }
        public TypeDevice TypeDevice { get; set; }
        public string DeviceId { get; set; }
        public string TokenFCM { get; set; }
        public DateTime? LoginDate { get; set; }
    }
}
