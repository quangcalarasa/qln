using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraHistoryAndStatus : AbstractEntity<int>
    {
        public string Code { get; set; } // ma dinh danh
        public string House { get; set; } // căn nhà
        public string Apartment { get; set; } // căn hộ
        public string Address { get; set; } // địa chỉ
        public double Total { get; set; }// diện tích
        public string Name { get; set; }//Tên chủ hộ
        public string Phone { get; set; }//điện thoại
        public DateTime ToDate { get; set; } //Ngày cập nhật
        public TypeUsageStatus TypeUsageStatusName { get; set; } //trạng thái
    }
}
