using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class Md167HouseData : Md167House
    {
        public string HouseTypeName { get; set; }// ten loai nha    
        public string ProviceName { get; set; }//ten tinh
        public string DistrictName { get; set; }//ten quan
        public string WardName { get; set; }//ten phuong
        public string LaneName { get; set; }//ten duong
        public string LaneNameStart { get; set; }//Đoạn đường từ
        public string LaneNameEnd { get; set; }//Đoạn đường đến
        public string fullAddress { get; set; }//dia chi day du
        public string LandTaxRateCode { get; set; }// thue xuat dat
        public string TypeLand { get; set; }//loai dat
        public string DecreeType { get; set; }//nghi dinh
        public string Md167TransferUnitName { get; set; }//don vi chuyen giao
        public string LocationType { get; set; }//vi tri dat o
        public string PurposeUsingName { get ; set; }// muc dich quan ly su dung 
        public string PlanContentName { get; set; }//Nội dung phương án được phê duyệt của UBND TP 
        public string StatusOfUseName { get; set; }// Hiện trạng sử dụng đất
        public bool? IsTownHouse { get; set; }  //Là nhà phố
        public bool? IsApartment { get; set; }  //Là nhà chung cư
        public decimal? AreaLandInBankSafeHouse { get; set; }// diện tích đất thuộc hành lang an toàn sông
        public decimal? AreaHouseInBankSafeHouse { get; set; }// diện tích nhà thuộc hành lang an toàn sông
        public decimal? AreaLandInBankSafeApartment { get; set; }// diện tích đất thuộc hành lang an toàn sông
        public decimal? AreaHouseInBankSafeApartment { get; set; }// diện tích nhà thuộc hành lang an toàn sông
        public double? UnitPriceValueTotal { get; set; }//
        public int? Index { get; set; }
        public bool Valid { get; set; }
        public string ErrMsg { get; set; }
    }
}
