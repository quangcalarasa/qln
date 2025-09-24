using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class TdcPlatformManagerData : TdcPlatformManager
    {
        public List<DistrictAllocasionPlatformData> districtAllocasionPlatform { get; set; }//danh sách quận/huyện và số lượng thực tế theo phân bổ (nền đất)
        public string TdcProjectName { get; set; }// tên dự án
        public string TdcLandName { get; set; }// tên lô
        public string TdcPlatformCode { get; set; }//mã nền
        public string TdcPlatformName { get; set; }// tên nền
        public string DistrictProjectName { get; set; }// Tên quận/huyện của dự án
        public string TypeDecisionName { get; set; }//tên quyết định
        public string TypeLegalName { get; set; }//tên mã pháp lý
        public int dictrictQuanCount { get; set; }
        public string districtAllocasionPlatformName { get; set; }

    }
    public class ListTypeData
    {
        public string TypeDecisionName { get; set; }//tên quyết định
        public string TypeLegalName { get; set; }//tên mã pháp lý
        public int TypeDecisionId { get; set; }//mã quyết định
        public int TypeLegalId { get; set; }//mã pháp lý tiếp nhận
    }
}