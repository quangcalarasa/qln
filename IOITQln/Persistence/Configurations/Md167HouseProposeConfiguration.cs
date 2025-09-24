using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167HouseProposeConfiguration : AppEntityTypeIntConfiguration<Md167HousePropose>
    {
        public override void Configure(EntityTypeBuilder<Md167HousePropose> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167HousePropose");
            builder.Property(c => c.Date).IsRequired();
            builder.Property(c => c.ProposeOption).IsRequired();
            builder.Property(c => c.BrowseStatus).IsRequired();
            builder.Property(c => c.Md167HouseId).IsRequired();
            builder.Property(c => c.BrowseDate).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
        }
    }
}
