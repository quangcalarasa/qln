using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class TypeBlockMap : AbstractEntity<int>            //bảng map loại nhà vs loại biên bản áp dụng
    {
        public int TypeBlockId { get; set; }
        public TypeReportApply TypeReportApply { get; set; }
    }
}
