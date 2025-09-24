using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Models.Data
{
    public class Md167ContractData : Md167Contract
    {
        public string DelegateName { get; set; }
        public string HouseCode { get; set; }
        public bool personOrCompany { get; set; }
        public string HouseNumber { get; set; }
        public string HouseNumberPar { get; set; }
        public bool isKios { get; set; }
        public string Lane { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public decimal? UseFloorPr { get; set; }
        //public Md167Delegate Delegate { get; set; }
        //public Md167House House { get; set; }
        public List<Md167PricePerMonth> pricePerMonths { get; set; }
        public List<Md167Valuation> valuations { get; set; }
        public List<Md167AuctionDecision> auctionDecisions { get; set; }
        public Md167Contract parent { get; set; }
        public List<Md167ContractData> extraData { get; set; }
        public bool Expand { get; set; }
    }

    public class GroupMd167DebtData
    {
        public int length { get; set; }
        public long? Md167ReceiptId { get; set; }
        public decimal? AmountPaidPerMonth { get; set; }      //Số tiền phải trả hàng tháng
        public decimal? AmountInterest { get; set; }        //Số tiền lãi phát sinh do chậm thanh toán
        public decimal? AmountPaidInPeriod { get; set; }    //Số tiền đến kỳ phải thanh toán
        public decimal? AmountToBePaid { get; set; }         //Số tiền phải thanh toán
        public decimal? AmountPaid { get; set; }            //Số tiền đã thanh toán
        public decimal? AmountDiff { get; set; }            //Chênh lệch
        public string Note { get; set; }
        public List<Md167Debt> dataGroup { get; set; }
    }

    //Data Nhà dùng trong hợp đồng
    public class Md167HouseInContractData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string HouseNumber { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public int? LaneId { get; set; }
        public decimal? UseFloorPr { get; set; }
        public string Address { get; set; }
        public Type_House TypeHouse { get; set; }
        public string ParentHouseNumber { get; set; }
        public decimal? AreaFloorBuild { get; set; }// diện tích sàn xây dựng
        public decimal? UseFloorPb { get; set; }//diện tích sử dụng sàn chung(Kios cũng dùng)
        public decimal? UseLandPb { get; set; }// diện tích sử dụng đất chung
        public decimal? UseLandPr { get; set; }// diện tích sử dụng đất riêng
        public decimal? AreBuildPb { get; set; }// diện tích xây dựng chung
        public decimal? AreBuildPr { get; set; }// diện tích xây dựng riêng

    }

    public class Md167ContractImportData
    {
        public int? Index { get; set; }
        public string Code { get; set; }
        public DateTime? DateSign { get; set; }
        public bool? IsPersonal { get; set; }
        public string OrganizationName { get; set; }
        public string TaxCode { get; set; }
        public string FullName { get; set; }
        public string IdentityCode { get; set; }
        public DateTime? IdentityDate { get; set; }
        public string IdentityPlace { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string BusinessLicense { get; set; }
        public string Position { get; set; }
        public string HouseNumber { get; set; }
        public string KiosNumber { get; set; }
        public string TypePrice { get; set; }
        public string RentalPeriod { get; set; }
        public string NoteRentalPeriod { get; set; }
        public string RentalPurpose { get; set; }
        public string NoteRentalPurpose { get; set; }
        public string PaymentPeriod { get; set; }
        public DateTime? DateGroundHandover { get; set; }
        public string ContractStatus { get; set; }
        public bool Valid { get; set; }
        public string ErrMsg { get; set; }
    }

    public class Md167ExtraContractImportData : Md167ContractImportData
    {
        public string ExtraCode { get; set; }
        public DateTime? ExtraDateSign { get; set; }
    }
}
