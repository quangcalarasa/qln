using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class CurrentStateMainTexture : AbstractEntity<int>
    {
        public int LevelBlock { get; set; }     //Khai báo bảng tỷ lệ chất lượng còn lại cho cấp của căn nhà
        public TypeMainTexTure TypeMainTexTure { get; set; }
        public string Name { get; set; }
        public bool? Default { get; set; }
        public string Note { get; set; }
    }
}
