using IOITQln.Common.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.Models.Dto
{
    public class LogActionModel: AbstractEntity<long>
    {
        public string ActionName { get; set; }
        public string TableName { get; set; }
        public long TargetId { get; set; }
        public string IpAddress { get; set; }
        public string Log { get; set; }
        public int Type { get; set; }

        public LogActionModel(string actionName, string tableName, long targetId, string ipAddress, string log, int type, int createById, string createbyName)
        {
            ActionName = actionName;
            TableName = tableName;
            TargetId = targetId;
            IpAddress = ipAddress;
            Log = log;
            Type = type;
            CreatedById = createById;
            CreatedBy = createbyName;
        }
    }
}
