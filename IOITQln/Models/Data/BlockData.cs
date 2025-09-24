using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class BlockData : Block
    {
        public string CustomerName { get; set; }
        public string FullAddress { get; set; }
        public List<LevelBlockMap> levelBlockMaps { get; set; }
        public List<BlockDetailData> blockDetails { get; set; }
        public List<BlockMaintextureRate> blockMaintextureRaties { get; set; }
        public List<DecreeMap> decreeMaps { get; set; }
        public List<ApartmentDetail> apartmentDetails { get; set; }
        public List<ApartmentDetailData> apartmentDetailData { get; set; }
        public List<ApartmentLandDetail> apartmentLandDetails { get; set; }
        //Thông tin diện đất liền kề đã bán
        public float? SellLandArea { get; set; }                        //Diện tích đất có nhà ở đã bán
        public List<PricingApartmentLandDetail> pricingApartmentLandDetails { get; set; }
        public string DistrictName { get; set; }
        public string LaneName { get; set; }
        public string WardName { get; set; }
        public string type_rpApply { get; set; }
        public string usage_Status { get; set; }
        public string TypeBlockName { get; set; }
        public string ProvinceName { get; set; }
        public string TypeHouseName { get; set; } // Trạng thái căn nhà
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
        public string ApartmentInfo { get; set; }
        public EditHistory editHistory { get; set; }
    }

    public class BlockDetailData: BlockDetail
    {
        public string FloorName { get; set; }
        public int FloorCode { get; set; }
        public string AreaName { get; set; }
        public bool? IsMezzanine { get; set; }
    }

    public class BlockMaintextureRateData: BlockMaintextureRate
    {
        public string TypeMainTexTureName { get; set; }
        public string CurrentStateMainTextureName { get; set; }
    }

    public class BlockMaintextureRateDataGroupByTypeMainTexTure
    {
        public TypeMainTexTure TypeMainTexTure { get; set; }
        public List<BlockMaintextureRateData> blockMaintextureRateDatas { get; set; }
    }

    public class BlockDataSm
    {
        public int Id { get; set; }
        public string Address { get; set; }
    }

    public class DataCodeHouse
    {
        public int Id { get; set; }
        public TypeReportApply TypeReportApply { get; set; }
        public int TypeBlockId { get; set; }
        public DataCodeHouse child { get; set; }

    }
}
