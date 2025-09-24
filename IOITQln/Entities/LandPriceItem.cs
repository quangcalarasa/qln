using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class LandPriceItem : AbstractEntity<int>           //Giá đất
    {
        public int LandPriceId { get; set; }
        public string LaneName { get; set; }             //Tên đường
        public string LaneStartName { get; set; }      //Đoạn đường từ
        public string LaneEndName { get; set; }      //Đoạn đường đến
        public double Value { get; set; }    //Giá
        public string Des { get; set; }     //Căn cứ theo
        public int Ward { get; set; }
    }
}
