using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Md167Receipt : AbstractEntity<long>
    {
        public int? Md167ContractId { get; set; }
        public decimal? Amount { get; set; }      //Số tiền
        public DateTime? DateOfPayment { get; set; }    //Ngày nộp tiền
        public DateTime? DateOfReceipt { get; set; }    //Ngày xuất hóa đơn
        public string ReceiptCode { get; set; }     //Số hóa đơn
        public string Note { get; set; }
        public bool? PaidDeposit { get; set; }      //Thanh toán tiền thế chân
    }
}
