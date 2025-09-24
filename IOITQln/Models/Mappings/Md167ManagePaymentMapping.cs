using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class Md167ManagePaymentMapping : Profile
    {
        public Md167ManagePaymentMapping()
        {
            CreateMap<Md167ManagePayment, Md167ManagePaymentData>();
            CreateMap<Md167ManagePaymentData, Md167ManagePayment>();
        }
    }
}
