using IOITQln.Entities;

namespace IOITQln.Models.Data
{
    public class FunctionData : Function
    {
    }

    public partial class FunctionTreeData
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? Level { get; set; }
        public bool? IsSpecialFunc { get; set; }
    }
}
