using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TypeAttributeItemConfiguration : AppEntityTypeIntConfiguration<TypeAttributeItem>
    {
        public override void Configure(EntityTypeBuilder<TypeAttributeItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("TypeAttributeItem");

            builder.Property(c => c.Code).HasMaxLength(200).IsRequired(true);
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired(true);
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
