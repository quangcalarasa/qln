using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraDebtNotification : AbstractEntity<int>
    {
        public string DebtType { get; set; }
        public TypeNotificationForm TypeNotificationForm { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string GroupNotification { get; set; }
        public string ListNotification { get; set; }
        public DateTime ToDate { get; set; } //Ngày yêu cầu
    }
}
