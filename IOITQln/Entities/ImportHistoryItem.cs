using IOITQln.Common.Bases;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ImportHistoryItem : AbstractEntity<Guid>
    {
        public long ImportHistoryId { get; set; }
        public string DataStorage { get; set; }

        [NotMapped]
        public dynamic Data
        {
            get
            {
                if (DataStorage != null && DataStorage != "")
                {
                    return JsonConvert.DeserializeObject<dynamic>(DataStorage);
                }
                else
                {
                    return new System.Dynamic.ExpandoObject();
                }
            }
            set
            {
                if (value != null)
                    DataStorage = JsonConvert.SerializeObject(value);
            }
        }
    }
}
