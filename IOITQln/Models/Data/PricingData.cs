using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class PricingData : Pricing
    {
        public Block block { get; set; }
        public Apartment apartment { get; set; }
        public Decree decree { get; set; }
        public List<PricingConstructionPrice> constructionPricies { get; set; }
        public List<PricingCustomer> customers { get; set; }
        public List<PricingLandTbl> landPricingTbl { get; set; }
        public List<PricingOfficer> pricingOfficers { get; set; }
        public List<PricingReducedPerson> reducedPerson { get; set; }
        public List<PricingApartmentLandDetail> pricingApartmentLandDetails { get; set; }
        public List<PricingReplaced> pricingReplaceds { get; set; }
        public EditHistory editHistory { get; set; }
    }

    public class PricingLandTblData: PricingLandTbl
    {
        public string AreaName { get; set; }
        public string PriceListItemNote { get; set; }
    }

    public class PricingReducedPersonData: PricingReducedPerson
    {
        public string CustomerName { get; set; }
    }

    //Nhóm PricingApartmentLandDetail theo Nghị định
    public class PricingApartmentLandDetailGroupByDecree
    {
        public DecreeEnum? Decree { get; set; }
        public string LandPriceItemNote { get; set; }
        public decimal? LandPriceItemValue { get; set; }
        public string? LandScapePrice { get; set; }
        public float? LandPriceRefinement { get; set; }
        public string PositionCoefficientStr { get; set; }
        public List<PricingApartmentLandDetail> pricingApartmentLandDetails { get; set; }
    }
}
