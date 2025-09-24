using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class InvestmentRateItem : AbstractEntity<int>           //Suất vốn đầu tư Item
    {
        public int InvestmentRateId { get; set; }
        public string LineInfo { get; set; }        //Dong so
        public string DetailInfo { get; set; }        //Chi tiet suất vốn
        public double Value { get; set; }    //Giá trị suất vốn đầu tư
        public double Value1 { get; set; }    //Giá trị xây dựng
        public double Value2 { get; set; }    //Giá trị thiết bị
    }
}
