using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    // Đơn vị đấu giá
    public class Md167Auctioneer : AbstractEntity<int>
    {
        public string Code { get; set; } // Mã đơn vị.
        public string AutInfo { get; set; }
        public string UnitName { get; set; } // Tên đơn vị.
        public string UnitAddress { get; set; } // Địa chỉ đơn vị.
        public string TaxNumber { get; set; } // Mã số thuế.
        public string BusinessLicense { get; set; } // Giấy phép kinh doanh.
        public string RepresentFullName { get; set; } // Họ tên người đại diện.
        public string RepresentPosition { get; set; } // Chức vụ của người đại diện.
        public string RepresentIDCard { get; set; } // Số chứng minh nhân dân của người đại diện.
        public DateTime RepresentDateOfIssue { get; set; } // Ngày cấp chứng minh nhân dân của người đại diện.
        public string RepresentPlaceOfIssue { get; set; } // Nơi cấp chứng minh nhân dân của người đại diện.
        public string ContactAddress { get; set; } // Địa chỉ liên hệ.
        public string ContactPhoneNumber { get; set; } // Số điện thoại liên hệ.
    }
}
