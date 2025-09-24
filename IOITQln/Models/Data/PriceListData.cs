using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class PriceListData : PriceList
    {
        //public string ParentName { get; set; }
        public string DecreeType1Name { get; set; }
        public string DecreeType2Name { get; set; }
        //public string UnitPriceName { get; set; }
        public List<PriceListItem> priceListItems { get; set; }
    }

    public class PriceListItemData: PriceListItem
    {
        public int DecreeType1Id { get; set; }
    }
}
