using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class SalaryCoefficient : AbstractEntity<int>           //Hệ số lương cơ bản
    {
        public int? DecreeType1Id { get; set; }
        public int DecreeType2Id { get; set; }
        public DateTime DoApply { get; set; }        //Ngày áp dụng
        public double Value { get; set; }    //Giá trị
        public string Note { get; set; }    //Diễn giải
    }
}
