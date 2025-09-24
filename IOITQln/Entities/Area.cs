using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Area : AbstractEntity<int>
    {
        //public TypeReportApply TypeReportApply { get; set; }
        //public int BlockId { get; set; }
        public int FloorId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool? IsMezzanine { get; set; }
        //public float AreaValue { get; set; }
        //public float GeneralAreaValue { get; set; }
        //public float PeronalAreaValue { get; set; }
    }
}
