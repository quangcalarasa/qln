using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167ManagePayment : AbstractEntity<int>
    {
        public string Code { get; set; } //mã phiếu chi
        public DateTime Date { get; set; }// ngày nộp phiếu chi
        public int Year { get; set; }//năm nộp
        public decimal Payment { get; set; }//tổng tiền thuế
        public string Note { get; set; }//ghi chú
    }
}
