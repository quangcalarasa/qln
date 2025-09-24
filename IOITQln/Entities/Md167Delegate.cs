using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    //Đại diện tổ chức
    public class Md167Delegate : AbstractEntity<int>
    {
        public string Code { get; set; }
        public personOrCompany PersonOrCompany { get; set; }
        public string? Name { get; set; }
        public string? NationalId { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public string? PlaceOfIssue { get; set; }
        public string? Address { get; set; }
        public string AutInfo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ComTaxNumber { get; set; }
        public string? ComBusinessLicense { get; set; } // Lưu trữ thông tin giấy phép kinh doanh.
        public string? ComOrganizationRepresentativeName { get; set; } // Lưu tên đại diện của tổ chức.
        public string? ComPosition { get; set; } // Chức vụ của người đại diện tổ chức.

    }
    public enum personOrCompany
    {
        PERSON=1,
        COMPANY=2
    }
}
