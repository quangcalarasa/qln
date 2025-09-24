using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class UseValueCoefficientItemConfiguration : AppEntityTypeIntConfiguration<UseValueCoefficientItem>
    {
        public override void Configure(EntityTypeBuilder<UseValueCoefficientItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("UseValueCoefficientItem");

            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
