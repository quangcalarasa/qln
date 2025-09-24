using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class VerifycationUnit : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public int? Type { get; set; }  //Loại văn bản xác minh
        public bool? VerifyStatus { get; set; }
    }
}
