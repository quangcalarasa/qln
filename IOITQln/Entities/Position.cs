using IOITQln.Common.Bases;

namespace IOITQln.Entities
{
    public class Position : AbstractEntity<int>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? DepartmentId { get; set; }
        public string AuthorDocsCode { get; set; }      //Mã văn bản ủy quyền
    }
}
