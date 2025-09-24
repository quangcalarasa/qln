using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class IdentifierData
    {
        public int? DistrictId { get; set; } //ID quận
        public string? DistrictName { get; set; }
        public int? TypeBlockId { get; set; } //Loại nhà
        public string TypeBlockName { get; set; } //Tên loại nhà
        public UsageStatus? UsageStatus { get; set; } //Tình trạng nhà
        public TypeHouse?  TypeHouse { get; set; } //Tình trạng nhà(BC số 3)
        public int? Quantity { get; set; }   // Số lượng
        public decimal? LandArea { get; set; } //Diện tích đất
        public decimal? ConstructionArea { get; set; } //Diện tích xây dựng
        public decimal?  UsableArea { get; set; } //Diện tích sử dụng,diện tích sàn
        public decimal? FloorNumber { get; set; } //Số tầng
        public bool? Dispute { get; set; }                  //Tranh chấp
        public bool? Blueprint { get; set; }                //Bản vẽ
        public bool? TakeOver { get; set; } //Chiếm dụng
        public bool? EstablishStateOwnership { get; set; } // Chưa xác nhập SHNN

        public decimal? AmountDue { get; set; }  // Số tiền phải thu
        public decimal? AmountReceived { get; set; }  // Số tiền đã thu
        public decimal Ratio { get; set; } //Tỷ lệ
        public decimal? AmountOwed { get; set; } //Số tiền còn nợ
        public decimal? VATPrice { get; set; } //Trích tiền nộp thuế VAT
        public decimal? Price1 { get; set; } //trích tiền nộp về sở
        public decimal? Price2 { get; set;  } //Chưa trích tiền nộp về sở
        public int Index { get; set; }
        public bool? check { get; set; } //Check đã nộp hay chưa (PLNOC7)
        public bool? CheckPayDepartment { get; set; } //Check đã nộp về sở hay chưa (PLNOC7)
        public int? BlockId { get; set; } //Id block (PLNOC1)
        public string Structure { get; set; } //Cấu trúc

        public int? Sell { get; set; } //Số lượng hộ bán
        public int? Rent { get; set; } //Số lượng hộ thuê
        public int? Empty { get; set; } //Số lượng hộ trống

        public string Code { get; set; } //Id hợp đồng pl7
    }

    public class GroupDataNOC
    {
        public int Index { get; set; }
        public int DistrictId { get; set; }
        public List<GroupByTypeBlockID> groupByDistrict { get; set; }
    }
    public class GroupByTypeBlockID
    {
        public int? TypeBlockID { get; set; }
        public List<GroupDataByTypeUsageStatus> groupByTypeBlockID { get; set; }
    }

    public class GroupDataByTypeUsageStatus
    {
        public UsageStatus? UsageStatus { get; set; }
        public List<IdentifierData> groupDataByTypeUsageStatus { get; set; }
    }

    public class GroupDataNOC4
    {
        public int Index { get; set; }
        public int DistrictId { get; set; }
        public List<GroupDataNOCs4> groupTypeBlockId { get; set; }
    }

    public class GroupDataNOCs4
    {
        public int? TypeBlockID { get; set; }

        public List<IdentifierData> groupDataNOC { get; set; }
    }


    public class GroupDataNOC3
    {
        public int Index { get; set; }
        public int DistrictId { get; set; }
        public List<GroupByTypeBlockID3> groupByTypeBlockID { get; set; }
    }
    public class GroupByTypeBlockID3
    {
        public int? TypeBlockID { get; set; }
        public decimal LandArea { get; set; }
        public decimal ConstructionArea { get; set; }
        public List<GroupDataByTypeTypeHouse> groupDataByTypeHouse { get; set; }
    }
    public class GroupDataByTypeTypeHouse
    {
        public TypeHouse? TypeHouse { get; set; }
        public decimal LandArea { get; set; }
        public decimal ConstructionArea { get; set; }
        public List<IdentifierData> data { get; set; }
    }

    public class GroupNOC2
    {
        public int? BlockId { get; set; }
        public List<IdentifierData> groupDataByBlockId { get; set; }
    }

    public class FillterNOC1
    {
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
        public int? TypeBlock { get; set; }
    }

    public class NOC7
    {
        public int Index { get; set; }
        public int DistrictId { get; set; }
        public List<NOC7ByTypeBlockID> noc7ByDistrict { get; set; }

    }

     public class NOC7ByTypeBlockID
    {
        public int? TypeBlockID { get; set; }
        public List<NOC7ByTypeUsageStatus> noc7ByTypeBlockID { get; set; }
    }

    public class NOC7ByTypeUsageStatus
    {
        public UsageStatus? UsageStatus { get; set; }
        public List<NOC7ByCode> noc7ByTypeUsageStatus { get; set; }
    }

    public class NOC7ByCode
    {
        public string Code { get; set; } //Id hợp đồng pl7
        public List<IdentifierData> Noc7byCode { get; set; }
    }

}
