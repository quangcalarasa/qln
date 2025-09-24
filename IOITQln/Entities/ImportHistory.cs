using IOITQln.Common.Bases;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ImportHistory : AbstractEntity<long>
    {
        public ImportHistoryType Type { get; set; }
        public string FileUrl { get; set; }
        public string DataStorage { get; set; }
        public string DataExtraStorage { get; set; }

        [NotMapped]
        public List<dynamic> Data
        {
            get
            {
                if(DataStorage != null && DataStorage != "")
                {
                    return JsonConvert.DeserializeObject<List<dynamic>>(DataStorage);
                }
                else
                {
                    return new List<dynamic>();
                }
            }
            set
            {
                if (value != null)
                    DataStorage = JsonConvert.SerializeObject(value);
            }
        }

        [NotMapped]
        public dynamic DataExtra
        {
            get
            {
                if (DataExtraStorage != null && DataExtraStorage != "")
                {
                    return JsonConvert.DeserializeObject<dynamic>(DataExtraStorage);
                }
                else
                {
                    return new System.Dynamic.ExpandoObject();
                }
            }
            set
            {
                if (value != null)
                    DataExtraStorage = JsonConvert.SerializeObject(value);
            }
        }
    }
}
