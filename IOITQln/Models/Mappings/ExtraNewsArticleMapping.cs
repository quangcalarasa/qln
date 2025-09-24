using AutoMapper;
using IOITQln.Entities;
using IOITQln.Models.Data;

namespace IOITQln.Models.Mappings
{
    public class ExtraNewsArticleMapping : Profile
    {
        public ExtraNewsArticleMapping()
        {
            CreateMap<ExtraNewsArticle, ExtraNewsArticleData>();
            CreateMap<ExtraNewsArticleData, ExtraNewsArticle>();
        }
    }
}
