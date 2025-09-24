using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class ConstructionPrice : AbstractEntity<int>        //Chỉ số giá xây dựng công trình
    {
        public int? ParentId { get; set; }
        public int? DecreeType1Id { get; set; }
        public int DecreeType2Id { get; set; }
        public string Des { get; set; }
        public string NameOfConstruction { get; set; }
        public string Note { get; set; }
        public int Year { get; set; }
        public int YearCompare { get; set; }
        public float Value { get; set; }
    }
}
