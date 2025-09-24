using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class LandPriceCorrectionCoefficient : AbstractEntity<int>           //Hệ số K điều chỉnh giá đất
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string Code { get; set; }     //Mã hệ số K
        public string Name { get; set; }        //Tên hệ số K
        public string Note { get; set; }     //Diễn giải
        public float Value { get; set; }    //Hệ số điều chỉnh vùng
        public float FacadeWidth { get; set; }      //Chiều rộng mặt tiền
    }
}
