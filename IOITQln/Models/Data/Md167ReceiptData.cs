using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class Md167ReceiptData : Md167Receipt
    {
        public string ContractCode { get; set; }
        public int? Index { get; set; }
        public bool Valid { get; set; }
        public string ErrMsg { get; set; }
    }
}
