using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraConfigDebtConfiguration : AppEntityTypeIntConfiguration<ExtraConfigDebt>
    {
        public override void Configure(EntityTypeBuilder<ExtraConfigDebt> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraConfigDebt");

            builder.Property(c => c.Date).IsRequired();
            builder.Property(c => c.DayOver).IsRequired(true);
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
        }
    }
}
