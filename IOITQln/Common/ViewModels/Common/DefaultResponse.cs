
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IOITQln.Common.ViewModels.Common
{
    public class DefaultResponse
    {
        public Meta meta { get; set; }
        public object data { get; set; }
        public object metadata { get; set; }
    }

    public class ExtraDefaultResponse
    {
        public Meta meta { get; set; }
        public object data { get; set; }
        public object metadata { get; set; }
        public object extradata { get; set; }
    }

    public class Meta
    {
        public int error_code { get; set; }
        public string error_message { get; set; }

        public Meta()
        { }

        public Meta(int errorCode, string errorMessage)
        {
            this.error_code = errorCode;
            this.error_message = errorMessage;
        }
    }

    public class Metadata
    {
        public int item_count { get; set; }
        public decimal total { get; set; }
        public Metadata()
        { }

        public Metadata(int item_count)
        {
            this.item_count = item_count;
        }
        public Metadata(decimal total)
        {
            this.total = total;
        }

        public Metadata(int item_count, decimal total)
        {
            this.item_count = item_count;
            this.total = total;
        }
    }
    

    public class MetadataTotal
    {
        public int Count { get; set; }
        public Nullable<decimal> TotalValuePlan { get; set; }
        public Nullable<decimal> TotalValueReal { get; set; }
        public Nullable<decimal> TotalValueAcceptance { get; set; }
        public Nullable<decimal> TotalValuePay { get; set; }
        public Nullable<decimal> TotalValuePlanOld { get; set; }
        public Nullable<decimal> TotalValueRealOld { get; set; }
        public Nullable<decimal> TotalValueAcceptanceOld { get; set; }
        public Nullable<decimal> TotalValuePayOld { get; set; }
        public Nullable<decimal> TotalValueContract { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string TotalDay { get; set; }

        public MetadataTotal()
        { }

        public MetadataTotal(decimal TotalValueAcceptance, decimal TotalValueAcceptanceOld, decimal TotalValuePay, decimal TotalValuePayOld)
        {
            this.TotalValueAcceptance = TotalValueAcceptance;
            this.TotalValueAcceptanceOld = TotalValueAcceptanceOld;
            this.TotalValuePay = TotalValuePay;
            this.TotalValuePayOld = TotalValuePayOld;
        }

        public MetadataTotal(decimal TotalValueContract, DateTime? DateStart, DateTime? DateEnd, string TotalDay)
        {
            this.TotalDay = TotalDay;
            this.TotalValueContract = TotalValueContract;
            this.DateStart = DateStart;
            this.DateEnd = DateEnd;
        }

    }


}