using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class TdcCustomerMapping : Profile
    {
        public TdcCustomerMapping()
        {
            CreateMap<TdcCustomerData, TdcCustomer>();
            CreateMap<TdcCustomer, TdcCustomerData>();

            CreateMap<TdcAuthCustomerDetail, TdcAuthCustomerDetailData>();
            CreateMap<TdcAuthCustomerDetailData, TdcAuthCustomerDetail>();

            CreateMap<TdcMemberCustomer, TdcMenberCustomerData>();
            CreateMap<TdcMenberCustomerData, TdcMemberCustomer>();
        }
    }
}
