using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TDCProjectData : TDCProject
    {
        public List<TDCProjectPriceAndTaxData> tDCProjectPriceAndTaxes { get; set; }
        public List<TDCProjectIngrePriceData> tDCProjectIngrePrices { get; set; }
        public List<TDCProjectData> tDCProjectByDistricts { get; set; }
        public string LaneName { get; set; }
        public string WardName { get; set;}
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
    }   
}
