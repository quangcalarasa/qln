using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraEmailDebt : AbstractEntity<int>
    {
        public int TemplateId { get; set; }
        public string Code { get; set; }// số hợp đồng
        public string Header { get; set; }
        public bool IsAuto { get; set; }
    }
}
