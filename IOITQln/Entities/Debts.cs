using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Debts : AbstractEntity<int>
    {
        public Guid RentFileId { get; set; } 
        public string Code { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal? SurplusBalance { get; set; } //Số dư treo
        public decimal? Total { get; set; } //Tổng tiền
        public decimal? Paid { get; set; } //Số tiền đã nộp
        public decimal? Diff { get; set; } //Số tiền còn nợ
        public int TypeBlockId { get; set; } //loại nhà
        public int DistrictId { get; set; } //ID quận
        public UsageStatus? UsageStatus { get; set; } //Tình trạng nhà
    }

}
