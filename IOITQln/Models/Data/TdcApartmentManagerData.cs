using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class TdcApartmentManagerData: TdcApartmentManager
    {
        public List<DistrictAllocasionApartmentData> districtAllocasionApartment { get; set; }//danh sách quận/huyện và số lượng thực tế theo phân bổ (căn hộ)
        public string TdcProjectName { get; set; }// tên dự án
        public string TdcLandName { get; set; }// tên lô
        public string TdcBlockHouseName { get; set; }// tên khối
        public string TdcFloorName { get; set; }// tên tầng
        public string TdcApartmentName { get; set; }// tên căn
        public string DistrictProjectName { get; set; }// Tên quận/huyện của dự án
        public string TypeDecisionName { get; set; }//tên quyết định
        public string TypeLegalName { get; set; }//tên mã pháp lý
        public string districtAllocasionApartmentName { get; set; }
    }

    public class GrByDistrictProjectId
    {
        public int DistrictProjectId { get; set; }
        public string DistrictName { get; set; }
        public List<GrByTdcProjectId> GrByTdcProjectIds { get; set; }
    }

    public class GrByTdcProjectId
    {
        public int TdcProjectId { get; set; }
        public string TdcProjectName { get; set; }

        public List<TdcApartmentManagerData> GrDataTdcApartmentManagerDatas { get; set; }
    }
}