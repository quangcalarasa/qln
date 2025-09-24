using IOITQln.Common.Bases;
using Org.BouncyCastle.Crypto.Prng.Drbg;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class TDCInstallmentPrice : AbstractEntity<int>
    {
        public int TdcCustomerId { get; set; }//id khách hàng
        public string ContractNumber { get; set; }//số hợp đồng
        public DateTime DateNumber { get; set; }//ngày hợp đồng
        public decimal NewContractValue { get; set; }//giá trị hợp đồng mới
        public decimal OldContractValue { get; set; }//giá trị hợp đồng cũ
        public decimal DifferenceValue { get; set; }// giá trị chênh lệch= NewContractValue - OldContractValue
        public string Floor1 { get; set; }//id lầu
        public int TdcProjectId { get; set; }//id dự án
        public int LandId { get; set; }//id lô
        public int PlatformId { get; set; }//id nền đất
        public int BlockHouseId { get; set; }//id
        public int FloorTdcId { get; set; }//id tầng
        public int TdcApartmentId { get; set; }   //id căn 
        public bool Corner { get; set; } // lô góc
        public string TemporaryDecreeNumber { get; set; }//số quyết định tạm thời
        public DateTime? TemporaryDecreeDate { get; set; }//ngày quyết định tạm thời
        public decimal? TemporaryTotalArea { get; set; }//Diện tích tạm thời
        public decimal? TemporaryTotalPrice { get; set; }//Thành tiền tạm thời
        public string DecreeNumber { get; set; }//số quyết định chính thức
        public DateTime DecreeDate { get; set; }//ngày quyết định chính thức
        public decimal TotalArea { get; set; }//Diện tích chính thức
        public decimal TotalPrice { get; set; }//Thành tiền chính thức
        public DateTime DateTDC { get; set; }
        public int YearPay { get; set; }  //số năm trả
        public decimal FirstPay { get; set; }// số tiền trả lần đầu
        public DateTime FirstPayDate { get; set; }// ngày tiền trả lần đầu
        public  decimal TotalPayValue { get; set; }// tổng số tiền phải trả
        public decimal PesonalTax { get; set; }// thuế thu nhập cá nhân
        public decimal RegistrationTax { get; set; }// thuế trước bạ
        //public decimal PublicBenefitValue { get; set; }// tiền nộp về công ích
        //public decimal CenterValue { get; set; }//tiền nộp về trung tâm
        public bool Check { get; set; } //Dùng để cho vào mảng check
        public bool isPayOff { get; set; }
        public int? ParentId { get; set; }
        public ContractTDCType Type { get; set; }

    }
}

