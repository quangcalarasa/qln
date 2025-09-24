using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class LandPriceItemData: LandPriceItem
    {
        public string DeslandPrice { get; set; }// căn cứ theo
        public string DeslandPriceNoneUnicode { get; set; }// căn cứ theo
        public int ProviceId { get; set; }//tên tỉnh thành phố id
        public int DistrictId { get; set; }//tên quận huyện id
        public int DecreeType1Id { get; set; }//nghị định id
        public int DecreeType2Id { get; set; }// văn bản pháp luật id
        public string ProviceName { get; set; }//tên tỉnh thành phố id
        public string DistrictName { get; set; }//tên quận huyện id
        public string DecreeType1Name { get; set; }//nghị định id
        public string DecreeType2Name { get; set; }// văn bản pháp luật id
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
