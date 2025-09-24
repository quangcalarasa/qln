using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class RentBctTableMapping : Profile
    {
        public RentBctTableMapping()
        {
            CreateMap<RentBctTable, RentBctTableData>();
            CreateMap<RentBctTableData, RentBctTable>();
        }
    }
}
