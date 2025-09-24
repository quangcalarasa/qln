using System;
using System.Runtime.CompilerServices;
using IOITQln.Common.Bases;
namespace IOITQln.Entities
{
    public class DebtsTable : AbstractEntity<int>
    {
        public string Code
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public DateTime DateStart
        {
            get;
            set;
        }

        public DateTime? DateEnd
        {
            get;
            set;
        }

        public Guid RentFileId
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public bool Check
        {
            get;
            set;
        }

        public string Executor
        {
            get;
            set;
        }

        public DateTime? Date
        {
            get;
            set;
        }

        public string NearestActivities
        {
            get;
            set;
        }

        public decimal PriceDiff
        {
            get;
            set;
        }

        public decimal AmountExclude
        {
            get;
            set;
        }

        public double VATPrice
        {
            get;
            set;
        }

        public double VAT
        {
            get;
            set;
        }

        public bool CheckPayDepartment
        {
            get;
            set;
        }

        public byte Type
        {
            get;
            set;
        }

        public Guid? NocReceiptId
        {
            get;
            set;
        }

        public string? SXN
        {

            get;

            set;
        }
    }
}
