using IOITQln.Common.Bases;
using Newtonsoft.Json;
using static IOITQln.Entities.Md167House;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace IOITQln.Entities
{
    public class Lane : AbstractEntity<int>     //Đường
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Ward { get; set; }
        public int District { get; set; }
        public int Province { get; set; }
        public string OldNameLane
        {
            get
            {
                if (InfoValue == null)
                {
                    InfoValue = new List<OldName>();
                }
                return JsonConvert.SerializeObject(InfoValue);
            }
            set
            {
                if (value != null)
                {
                    InfoValue = JsonConvert.DeserializeObject<List<OldName>>(value);
                }
                else
                {
                    InfoValue = new List<OldName>();
                }
            }
        }
        [NotMapped]
        public List<OldName>? InfoValue { get; set; } = new List<OldName>();
        public class OldName
        {
            public string Name { get; set; }
        }
    }
}
