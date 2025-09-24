using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class DeductionCoefficient : AbstractEntity<int>           //Hệ số được giảm
    {
        public int? DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string ObjectApply { get; set; }     //Đối tượng áp dụng
        public double Value { get; set; }    //Giá trị
        public string Note { get; set; }    //Diễn giải
    }
}
