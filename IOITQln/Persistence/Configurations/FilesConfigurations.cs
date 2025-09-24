using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class FilesConfigurations : AppEntityTypeIntConfiguration<Files>
    {
        public override void Configure(EntityTypeBuilder<Files> builder)
        {
            base.Configure(builder);
            builder.ToTable("Files");
        }
    }
}
