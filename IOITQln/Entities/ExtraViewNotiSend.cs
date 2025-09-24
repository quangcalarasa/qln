using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraViewNotiSend : AbstractEntity<int>
    {
        public string NotificationTitle { get; set; }
        public string ContentNotification { get; set; }
        public string TypeNotification { get; set; }
        public int SendNotiBy { get; set; }
        public DateTime DateSend { get; set; }
    }
}
