using IOITQln.Common.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167Debt : AbstractEntity<long>
    {
        public string HouseCode { get; set; }
        public int? Md167ContractId { get; set; }
        public Md167DebtTypeRow TypeRow { get; set; }               //Loại dòng: Dòng thế chân, dòng dữ liệu mỗi kỳ, dòng năm, dòng tổng
        public int? Index { get; set; }                 //Lần trả
        public string Title { get; set; }
        public DateTime? Dop { get; set; }      //Ngày thanh toán theo quy định
        public DateTime? DopExpected { get; set; }      //Ngày thanh toán dự kiến
        public DateTime? DopActual { get; set; }        //Ngày thanh toán thực tế
        public int? InterestCalcDate { get; set; }      //Số ngày tính lãi suất
        public float? Interest { get; set; }       //Lãi suất tính theo ngày
        public decimal? AmountPaidPerMonth { get; set; }      //Số tiền phải trả hàng tháng
        public decimal? AmountInterest { get; set; }     //Số tiền lãi phát sinh do chậm thanh toán
        public decimal? AmountPaidInPeriod { get; set; } //Số tiền đến kỳ phải thanh toán
        public decimal? AmountToBePaid { get; set; }         //Số tiền phải thanh toán
        public decimal? AmountPaid { get; set; }            //Số tiền đã thanh toán
        public decimal? AmountDiff { get; set; }            //Chênh lệch
        public string Note { get; set; }                    //Ghi chú
        public long? Md167ReceiptId { get; set; }           //Id phiếu thu
    }
}
