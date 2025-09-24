using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class ApartmentData : Apartment
    {
        public string CustomerName { get; set; }
        public string BlockName { get; set; }
        public string BlockAddress { get; set; }
        public List<ApartmentDetail> apartmentDetails { get; set; }
        public List<ApartmentDetailData> apartmentDetailData { get; set; }
        public List<BlockMaintextureRate> blockMaintextureRaties { get; set; }
        public List<ApartmentLandDetail> apartmentLandDetails { get; set; }
        //Thông tin diện đất liền kề đã bán
        public float? SellLandArea { get; set; }                        //Diện tích đất có nhà ở đã bán
        public List<PricingApartmentLandDetail> pricingApartmentLandDetails { get; set; }
        public int DistrictId { get; set; }

        public string DistrictName { get; set; }
        public int TypeBlockId { get; set; }
        public EditHistory editHistory { get; set; }
        public int? Lane { get; set; }
        public int? Ward { get; set; }
        public int? District { get; set; }
    }

    public class ApartmentDetailData : ApartmentDetail
    {
        public string FloorName { get; set; }
        public int FloorCode { get; set; }
        public string AreaName { get; set; }
        public bool? IsMezzanine { get; set; }
    }

    public class ApartmentDetailDataGroupByLevel
    {
        public int? Level { get; set; }
        public List<ApartmentDetailData> apartmentDetailDatas { get; set; }
    }
}
