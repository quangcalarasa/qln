using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class TypeBlock : AbstractEntity<int>            //bảng loại nhà
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
    }
}
