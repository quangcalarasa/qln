using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class EditHistory : AbstractEntity<long>
    {
        public string ContentUpdate { get; set; }
        public string ReasonUpdate { get; set; }
        public string AttactmentUpdate { get; set; }
        public long TargetId { get; set; }
        public TypeEditHistory TypeEditHistory { get; set; }
        public Guid? RentFileId { get; set; }
    }
}
