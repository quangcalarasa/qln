using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class UseValueCoefficient : AbstractEntity<int>        //Hệ số điều chỉnh giá trị sử dụng
    {
        //public TypeReportApply TypeReportApply { get; set; }
        public int DecreeType1Id { get; set; }
        public int DecreeType2Id { get; set; }
        public string Des { get; set; }     //Căn cứ
    }
}
