using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class HousePaymentMapping: Profile
    {
        public HousePaymentMapping() 
        {
            CreateMap<HousePayment, HousePaymentData>();
            CreateMap<HousePaymentData, HousePayment>();
        }
    }
}
