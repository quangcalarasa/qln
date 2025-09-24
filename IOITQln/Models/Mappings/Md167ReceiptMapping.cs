using IOITQln.Entities;
using IOITQln.Models.Data;
using AutoMapper;

namespace IOITQln.Models.Mappings
{
    public class Md167ReceiptMapping : Profile
    {
        public Md167ReceiptMapping()
        {
            CreateMap<Md167Receipt, Md167ReceiptData>();
            CreateMap<Md167ReceiptData, Md167Receipt>();
        }
    }
}
