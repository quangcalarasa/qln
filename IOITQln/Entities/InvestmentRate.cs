using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class InvestmentRate : AbstractEntity<int>           //Suất vốn đầu tư
    {
        public TypeReportApply TypeReportApply { get; set; }
        public int? DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        //public string Code { get; set; }        //Ký hiệu
        //public string Name { get; set; }        //Tên suất vốn
        public string Des { get; set; }     //Căn cứ theo
        //public double Value { get; set; }    //Giá trị suất vốn đầu tư
        //public double Value1 { get; set; }    //Giá trị xây dựng
        //public double Value2 { get; set; }    //Giá trị thiết bị
    }
}
