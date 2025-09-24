using IOITQln.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Common.ViewModels.Common
{
    public class FilteredPagination : BasePagination
    {
        [System.ComponentModel.DefaultValue("1=1")]
        public string query { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string select { get; set; }

        [System.ComponentModel.DefaultValue("")]
        public string search { get; set; }
    }
    public class FilteredLandPricePagination : FilteredPagination
    {
        public landPriceType LandPriceType { get; set; }
    }
    public class FilteredMd167HousePagination : FilteredPagination
    {
        public Type_House TypeHouse { get; set; }
    }
}