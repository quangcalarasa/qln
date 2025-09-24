using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class PriceList : AbstractEntity<int>        //Bảng giá tính nhà ở
    {
        //public int? ParentId { get; set; }
        public string Des { get; set; }
        //public string NameOfConstruction { get; set; }
        //public bool? IsMezzanine { get; set; }
        //public string Note { get; set; }
        //public int? UnitPriceId { get; set; }
        public int DecreeType1Id { get; set; }      //Nghị định
        public int DecreeType2Id { get; set; }      //Thông tư, văn bản
        //public double ValueTypePile1 { get; set; }       //Giá trị Móng cọc các loại L <= 15
        //public double ValueTypePile2 { get; set; }       //Giá trị Móng cọc các loại L > 15
    }
}
