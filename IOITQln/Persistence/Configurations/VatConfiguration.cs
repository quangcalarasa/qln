using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class VatConfiguration : AppEntityTypeIntConfiguration<Vat>
    {
        public override void Configure(EntityTypeBuilder<Vat> builder)
        {
            base.Configure(builder);
            builder.ToTable("Vat");

            builder.Property(c => c.Note).HasMaxLength(4000);
        }
    }
}
