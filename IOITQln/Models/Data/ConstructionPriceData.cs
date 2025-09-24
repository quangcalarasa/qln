using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class ConstructionPriceData : ConstructionPrice
    {
        public string ParentName { get; set; }
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        //public List<ConstructionPriceItem> constructionPriceItems { get; set; }
    }
}
