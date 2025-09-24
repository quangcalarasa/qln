using IOITQln.Entities;
using System;

namespace IOITQln.Models.Dto
{
    public class PromissoryReportRes : NocReceipt
    {
        public Guid? RentFileId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string ContractNo { get; set; }
        public decimal? AmountNoTax { get; set; }
        public decimal? AmountTax { get; set; }
        public string ContentsVat { get; set; }
    }
}
