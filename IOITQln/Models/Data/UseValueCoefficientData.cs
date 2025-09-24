using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class UseValueCoefficientData : UseValueCoefficient
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        public List<UseValueCoefficientItem> useValueCoefficientItems { get; set; }
        public List<UseValueCoefficientTypeReportApply> useValueCoefficientTypeReportApplies { get; set; }
    }

    public class UseValueCoefficientItemData: UseValueCoefficientItem
    {
        public int DecreeType1Id { get; set; }
        public TypeReportApply TypeReportApply { get; set; }
    }
}
