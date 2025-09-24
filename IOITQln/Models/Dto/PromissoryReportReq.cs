using System;

namespace IOITQln.Models.Dto
{
    public class PromissoryReportReq
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        //public string Code { get; set; }
        public float? Vat { get;set; }
    }
}
