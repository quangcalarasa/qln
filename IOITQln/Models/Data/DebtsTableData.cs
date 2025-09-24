using DevExpress.Office.Utils;
using IOITQln.Entities;
using System;
using System.Collections.Generic;


namespace IOITQln.Models.Data
{
    public class DebtsTableData : DebtsTable
    {

    }

    public class GroupDataDebtTableByCode
    {
        public Guid RentFileID { get; set; }
        public byte Type { get; set; }
        public byte TypeRentFile { get; set; }
        public List<groupDataDebtsByDate> groupDataDebtsByDates { get; set; }
    }

    public class groupDataDebtsByDate
    {
        public DateTime DateStart { get; set; }
        public decimal PriceTotal { get; set; }
        public DateTime DateEnd { get; set; }
        public decimal Price { get; set; }
        public bool Check { get; set; }
        public string Executor { get; set; } //Người thực hiện
        public DateTime? Date { get; set; } //Ngày thực hiện
        public string NearestActivities { get; set; } //hành động gần nhất
        public decimal PriceDiff { get; set; }  //Số tiền còn nợ
        public decimal AmountExclude { get; set; } //Tiền trước thuế
        public double VATPrice { get; set; } //VAT
        public bool CheckPayDepartment { get; set; }
        public List<DebtsTable> groupDebtTables { get; set; }
        public int index { get; set; }
        public string SXN { get; set; }
    }
}
