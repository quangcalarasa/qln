using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class LandPrice : AbstractEntity<int>           //Giá đất
    {
        public int DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        //public int LaneId { get; set; }             //Tên đường
        //public int LaneStartId { get; set; }      //Đoạn đường từ
        //public int LaneEndId { get; set; }      //Đoạn đường đến
        //public int UnitPriceId { get; set; }      //Đoạn đường đến
        public double? MinValue { get; set; }    //Giá đất tối thiểu
        public string Des { get; set; }     //Căn cứ theo
        public int District { get; set; }
        public int Province { get; set; }
        public landPriceType LandPriceType { get; set; }

    }
    public enum landPriceType
    {
        NOC=1,
        MD167=2
    }
}
