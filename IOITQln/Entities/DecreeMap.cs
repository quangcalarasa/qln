using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class DecreeMap : AbstractEntity<long>
    {
        public TypeDecreeMapping Type { get; set; }
        public long TargetId { get; set; }
        public DecreeEnum DecreeType1Id { get; set; }
    }
}
