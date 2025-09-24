using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraEmailNoti : AbstractEntity<int>
    {
        public string HostSent { get; set; }
        public string EmailSent { get; set; }
        public string SenderName { get; set; }
        public string PassHashSent { get; set; }
        public string PortSentEmail { get; set; }
    }
}
