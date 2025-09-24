//using IOITQln.Common.Bases.Configurations;
//using IOITQln.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace IOITQln.Persistence.Configurations
//{
//    public class ConstructionPriceItemConfiguration : AppEntityTypeIntConfiguration<ConstructionPriceItem>
//    {
//        public override void Configure(EntityTypeBuilder<ConstructionPriceItem> builder)
//        {
//            base.Configure(builder);
//            builder.ToTable("ConstructionPriceItem");

//            builder.Property(c => c.ConstructionPriceId).IsRequired(true);
//            builder.Property(c => c.Title).HasMaxLength(2000).IsRequired(true);
//            builder.Property(c => c.Value).IsRequired(true);
//        }
//    }
//}
