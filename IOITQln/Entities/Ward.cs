using IOITQln.Common.Bases;
using Newtonsoft.Json;
using static IOITQln.Entities.Lane;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOITQln.Entities
{
    public class Ward : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public string OldName
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
    }
}
