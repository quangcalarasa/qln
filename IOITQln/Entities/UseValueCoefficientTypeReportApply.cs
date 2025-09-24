using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class UseValueCoefficientTypeReportApply : AbstractEntity<int>        //Hệ số điều chỉnh giá trị sử dụng map với loại Biên bản
    {
        public int UseValueCoefficientId { get; set; }
        public TypeReportApply TypeReportApply { get; set; }
    }
}
