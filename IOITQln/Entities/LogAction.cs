using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class LogAction : AbstractEntity<long>
    {
        public string ActionName { get; set; }
        public string TableName { get; set; }
        public long TargetId { get; set; }
        public string IpAddress { get; set; }
        public string Log { get; set; }
        public Action? Type { get; set; } 
    }
}
