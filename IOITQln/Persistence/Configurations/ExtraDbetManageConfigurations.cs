using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraDbetManageConfigurations : AppEntityTypeIntConfiguration<ExtraDbetManage>
    {
        public override void Configure(EntityTypeBuilder<ExtraDbetManage> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraDbetManage");

            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");

        }
    }
}
