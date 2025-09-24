using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class SyntheticReportData
    {
        public int? TypeDecisionId { get; set; }//mã quyết định
        public int? TypeLegalId { get; set; }//mã quyết định
        public int? LandId { get; set; }//mã lô
        public int? BlockHouseId { get; set; } //mã khối
        public int? ApartmentTdcId { get; set; }// mã căn
        public int? FloorTdcId { get; set; }//mã tầng
        public int? TdcProjectId { get; set; }//mã dự án
        public int? TdcProjectIdApartment { get; set; }//mã dự án theo căn
        public int? TdcProjectIdPlatform { get; set; }//mã dự án nền
        public int? PlatformTdcId { get; set; }// mã căn
        public int? DistrictId { get; set; }//Mã quận/huyện
        public int? DistrictProjectId { get; set; }//Mã quận/huyện dự án
        public string TdcProjectName { get; set; }// tên dự án
        public string TdcProjectNameApartment { get; set; }// tên dự án theo căn
        public string TdcProjectNamePlatform { get; set; }// tên dự án theo nền
        public string TdcLandName { get; set; }// tên lô
        public string TdcBlockHouseName { get; set; }// tên khối
        public string TdcFloorName { get; set; }// tên tầng
        public string TdcApartmentName { get; set; }// tên căn hộ
        public string TdcPlatformName { get; set; }// tên nền đất
        public string DistrictName { get; set; }// tên quận/huyện
        public string DistrictProjectName { get; set; }// Tên quận/huyện của dự án
        public string TypeDecisionName { get; set; }//tên quyết định
        public string TypeLegalName { get; set; }//tên quyết định
        public TypeReception? ReceptionTimeApartment { get; set; }//Chọn tiếp nhận căn hộ
        public TypeReception? ReceptionTimePlatform { get; set; }//Chọn tiếp nhận căn hộ
        public string HandoverYearApartment { get; set; }//Năm bàn giao của căn hộ
        public string HandoverYearPlatform { get; set; }//Năm bàn giao của nền đất

    }
}
