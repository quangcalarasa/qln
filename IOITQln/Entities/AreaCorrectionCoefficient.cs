using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class AreaCorrectionCoefficient : AbstractEntity<int>           //Hệ số điều chỉnh vùng
    {
        public int? ParentId { get; set; }
        public int DecreeType1Id { get; set; }      //Nghị định
        public int DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string Des { get; set; }     //Căn cứ theo
        public string Name { get; set; }        //Tên hệ số điều chỉnh vùng
        public string Note { get; set; }     //Diễn giải
        public double Value { get; set; }    //Hệ số điều chỉnh vùng
        public DateTime? DoApply { get; set; }        //Ngày áp dụng
    }
}
