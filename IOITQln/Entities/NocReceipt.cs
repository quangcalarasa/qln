using DevExpress.XtraEditors.Filtering.Templates;
using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class NocReceipt : AbstractEntity<Guid>
    {
        public string Number { get; set; }
        public string Code { get; set; }                    //Mã định danh
        public DateTime Date { get; set; }                  //Ngày thực hiện
        public string Executor { get; set; }                //Người thực hiện
        public decimal Price { get; set; }                  //Số tiền
        public byte Action { get; set; }                    //Hành động
        public string NumberOfTransfer { get; set; }        //Số phiếu chuyển
        public string InvoiceCode { get; set; }             //Mã xuất hóa đơn
        public DateTime? DateOfTransfer { get; set; }       //Ngày phiếu chuyển
        public string Note { get; set; }
        public string Content { get; set; }
        public bool? IsImportExcel { get; set; }
    }
}
