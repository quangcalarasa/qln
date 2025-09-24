using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class Md167KiosData : Md167House
    {
        public string CodeHouse { get; set; }// Mã nhà
        public string CodeKios { get ; set; }//Mã kios
        public decimal? UseFloorPb { get; set; }//diện tích sử dụng sàn chung(Kios cũng dùng)
        public decimal? UseFloorPr { get; set; }//dienj tích sử dụng sàn riêng(Kios cũng dùng)
        public string KiosType { get; set; }// trạng thái kios
        public bool? HouseType { get; set; }//loại nhà chung cư hay nhà phố
        public int? Index { get; set; }
        public bool Valid { get; set; }
        public string ErrMsg { get; set; }
    }
}
