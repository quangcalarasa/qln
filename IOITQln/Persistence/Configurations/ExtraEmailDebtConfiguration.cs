using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraEmailDebtConfiguration : AppEntityTypeIntConfiguration<ExtraEmailDebt>
    {
        public override void Configure(EntityTypeBuilder<ExtraEmailDebt> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraEmailDebt");

            builder.Property(c => c.Code).IsRequired();
            builder.Property(c => c.Header).IsRequired();
            builder.Property(c => c.TemplateId).IsRequired(true);
            builder.Property(c => c.IsAuto).IsRequired().HasDefaultValue(true);
        }
    }
}
