using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TDCProjectIngrePriceConfiguration : AppEntityTypeIntConfiguration<TDCProjectIngrePrice>
    {
        public override void Configure(EntityTypeBuilder<TDCProjectIngrePrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("TDCProjectIngrePrice");

            builder.Property(c=> c.TDCProjectId).IsRequired();
            builder.Property(c => c.IngredientsPriceId).IsRequired();
            builder.Property(c => c.Value).IsRequired();
            builder.Property(c => c.Location).IsRequired();
        }
    }
}
