using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCProjectPriceAndTaxDetailsConfiguration : AppEntityTypeIntConfiguration<TDCProjectPriceAndTaxDetails>
    {
        public override void Configure(EntityTypeBuilder<TDCProjectPriceAndTaxDetails> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCProjectPriceAndTaxDetails");

            builder.Property(c => c.IngredientsPriceId).IsRequired();
            builder.Property(c => c.PriceAndTaxId).IsRequired();
        }
    }
}
