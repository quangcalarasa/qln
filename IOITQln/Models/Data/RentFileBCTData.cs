using DevExpress.Office.Utils;
using IOITQln.Entities;
using System;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class RentFileBCTData : RentFileBCT
    {
        public List<GroupData> groupData { get; set; }

    }

    public class BCT
    {
        public int? TypeQD { get; set; }
        public int GroupId { get; set; } 
        public string AreaName { get; set; } // Loại nhà
        public int? Level { get; set; } //Cấp nhà
        public float? PrivateArea { get; set; }// Diện tích sử dụng riêng
        public DateTime? DateCoefficient { get; set; } //Thời điểm bố trí
        public decimal? StandardPrice { get; set; } // Giá chuẩn
        public List<chilDf> chilDfs { get; set; } //List 7 bảng
        public float? TotalK { get; set; } //Tổng K
        public decimal?   Ktlcb { get; set; }  //Hệ số lương cơ bản
        public decimal? Ktdbt { get; set; }  //Hệ số bố trí sử dụng
        public decimal? PriceRent1m2 { get; set; } //giá thuê 1m2/tháng
        public decimal? PriceRent { get; set; } //Giá thuê
        public string Unit { get; set ;} //Đơn vị
        public DateTime? DateStart { get; set; } //Ngày bắt đầu
        public DateTime? DateEnd { get; set; } //Ngày kết thúc
        public int DateDiff { get; set; } //Ngày chênh lệch giữ start và end
        public string Note { get; set; } //Ghi chú
        public double VAT { get; set; } //VAT
        public decimal PolicyReduction { get; set; } //Giảm chính sách
        public bool check { get; set; }
        public Guid RentFileId  { get; set; }
        public decimal PriceVAT { get; set; }
        public int MonthDiff { get; set; } //Số tháng chênh lệch
        public decimal TotalPrice { get; set; } //Số tiền tổng(dùng cho báo cáo)
        public decimal? PriceAfterDiscount { get; set; } //Số tiền theo tháng sau giảm giá
        public decimal? DiscountCoff { get; set; } //Hệ số giảm giá
    } 
    public class chilDf
    {
        public TypeTable CoefficientId { get; set; }
        public double? Value { get; set; }
        public string? Code { get; set; }
    }

    public class changeValue
    {
        public byte Type { get; set; } //1-VAT,2-K.LCB,3-K.TDBT,4-HSGG,5-toltalK;
        public DateTime DateChange { get; set; }
        public double valueChange { get; set; }
    }

    public class changeK
    {
        public TypeTable CoefficientId { get; set; }
        public DateTime DateChange { get; set; }
        public double valueChange { get; set; }
    }

    public class groupChangeK
    {
        public DateTime DateChange { get; set; }
        public List<changeK> groupDataChangeK { get; set; }
    }
    public class groupData
    {
        public DateTime DateStart { get; set; } //Ngày bắt đầu
        public DateTime? DateEnd { get; set; } //Ngày kết thúc
        public int DateDiff { get; set; } //Ngày chênh lệch giữ start và end
        public int GroupId { get; set; }
        public decimal totalPrice { get; set; } //Tổng số tiền theo kì
        public double VAT { get; set; }
        public Guid rentFileId { get; set; }
        public List<BCT> bCTs { get; set; }
        public List<RentBctTableData> Bcts { get; set; }

        public string TypeBlockName { get; set; }    
    }

    public class groupdataCN
    {
        public Guid RentFileId { get; set; }
        public byte Type { get; set; }
        public List<groupByDate> groupByDates { get; set; }
    }

    public class groupByDate
    {
        public DateTime? DateStart { get; set; }
        public decimal Total { get; set; }
        public double VAT { get; set; }
        public DateTime? DateEnd { get; set; } //Ngày kết thúc

        public List<BCT> bCTs { get; set; }
    }
}
