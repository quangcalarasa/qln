using IOITQln.Common.Bases.Configurations;
using IOITQln.Controllers.ApiTdc;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class AnnualInstallmentConfiguration : AppEntityTypeIntConfiguration<AnnualInstallment>
    {
        public override void Configure(EntityTypeBuilder<AnnualInstallment> builder)
        {
            base.Configure(builder);
            builder.ToTable("AnnualInstallment");

            builder.Property(c => c.UnitPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
