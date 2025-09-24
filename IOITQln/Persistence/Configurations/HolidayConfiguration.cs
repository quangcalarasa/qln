using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class HolidayConfiguration : AppEntityTypeIntConfiguration<Holiday>
    {
        public override void Configure(EntityTypeBuilder<Holiday> builder)
        {
            base.Configure(builder);
            builder.ToTable("Holiday");

            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
