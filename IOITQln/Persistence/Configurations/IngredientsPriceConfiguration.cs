using IOITQln.Common.Bases.Configurations;
using IOITQln.Controllers.ApiTdc;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class IngredientsPriceConfiguration : AppEntityTypeIntConfiguration<IngredientsPrice>
    {
        public override void Configure(EntityTypeBuilder<IngredientsPrice> builder)
        {
            base.Configure(builder);
            builder.ToTable("IngredientsPrice");
            
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
