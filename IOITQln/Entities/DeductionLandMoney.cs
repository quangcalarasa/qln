using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class DeductionLandMoney : AbstractEntity<int>   //Tiền đất miễn giảm
    {
        public int? DecreeType1Id { get; set; }      //Nghị định
        public int DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string Condition { get; set; }     //Điều kiện
        public string Note { get; set; }     //Diễn giải
        public float Value { get; set; }    //Giá trị
    }
}
