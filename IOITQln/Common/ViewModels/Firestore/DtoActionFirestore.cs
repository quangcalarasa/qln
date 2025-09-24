using System;
using System.Collections.Generic;
using System.Text;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Common.ViewModels.Firestore
{
    public class DtoActionFirestore
    {
        public Guid ActionId { get; set; }
        public string ActionName { get; set; }
        public string ActionType { get; set; }
        public string TargetId { get; set; }
        public string TargetType { get; set; }
        public int? TargetAction { get; set; }
        public int? ProjectId { get; set; }
        public string Logs { get; set; }
        public int? Type { get; set; }
        public long? UserId { get; set; }
        public EntityStatus? Status { get; set; }
    }
}
