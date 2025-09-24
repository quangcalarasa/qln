using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class LandPriceData : LandPrice
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictName { get; set; }
        //public string LaneEndName { get; set; }
        //public string UnitPriceName { get; set; }
        public List<LandPriceItem> landPriceItems { get; set; }
    }

    public class LandPriceItemDecreeData
    {
        public int Id { get; set; }
        public string LaneName { get; set; }
        public double? Value { get; set; }
        public int? Ward { get; set; }
        public int? DecreeType1Id { get; set; }
    } 
}
