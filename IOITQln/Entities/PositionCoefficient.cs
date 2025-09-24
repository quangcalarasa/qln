using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class PositionCoefficient : AbstractEntity<int>      //Hệ số vị trí
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string Name { get; set; }            //Tên hệ số vị trí
        public DateTime DoApply { get; set; }        //Ngày áp dụng
        public float LocationValue1 { get; set; }       //Vị trí 1
        public float LocationValue2 { get; set; }       //Vị trí 2
        public float LocationValue3 { get; set; }       //Vị trí 3
        public float LocationValue4 { get; set; }       //Vị trí 4
        public float AlleyValue1 { get; set; }          //Hẻm vị trí 1
        public float AlleyValue2 { get; set; }          //Hẻm vị trí 2
        public float AlleyValue3 { get; set; }          //Hẻm vị trí 3
        public float AlleyValue4 { get; set; }          //Hẻm vị trí 4
        public float? AlleyLevel2 { get; set; }          //Hẻm cấp 2
        public float? AlleyOther { get; set; }           //Hẻm cấp còn lại
        public float? AlleyLand { get; set; }            //Hẻm trải đất
        public float? PositionDeep { get; set; }            //Độ sâu vị trí
        public float? LandPriceRefinement { get; set; }     //Giá đất tinh giảm
    }
}
