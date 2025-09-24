using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class InvestmentRateData : InvestmentRate
    {
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        public List<InvestmentRateItem> investmentRateItems { get; set; }
    }

    public class InvestmentRateItemData : InvestmentRateItem
    {
        public TypeReportApply TypeReportApply { get; set; }
        public int? DecreeType1Id { get; set; }
        public string Des { get; set; }
    }
}
