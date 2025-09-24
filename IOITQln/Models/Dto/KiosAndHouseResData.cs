using static IOITQln.Entities.Md167House;

namespace IOITQln.Models.Dto
{
    public class KiosAndHouseResData
    {
        public int Year { get; set; }
        public int? HouseId { get; set; }
        public string HouseName { get; set;}
        public string HouseCode { get; set;}
        public int ProviceId { get; set;}
        public int DistrictId { get; set; }
        public int WardId { get; set;}
        public int LaneId { get; set;}
        public int? Md167HouseId { get; set;}
        public string ProviceName { get; set;}
        public string DistrictName { get; set;}
        public string WardName { get; set;}
        public string LaneName { get; set;}
        public decimal? TaxNN { get; set; }//thuế đất phí nông nghiệp của nhà đất(Nhà đất và kios dùng)
        public Type_House TypeHouse { get; set; }
        public bool IsPayTax { get; set;}
        public bool isKios { get; set; }
        public string fullAddress { get; set;}
    }
}
