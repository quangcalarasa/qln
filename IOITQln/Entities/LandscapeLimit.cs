using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class LandscapeLimit : AbstractEntity<int>           //Hạn mức đất ở
    {
        public TypeReportApply TypeReportApply { get; set; }
        public int? DecreeType1Id { get; set; }      //Nghị định
        public int? DecreeType2Id { get; set; }      //Thông tư, văn bản
        public string Note { get; set; }     //Diễn giải
        public int? ProvinceId { get; set; }    //Tỉnh/Tp
    }
}
