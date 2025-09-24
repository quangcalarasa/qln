using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TypeBlockConfiguration : AppEntityTypeIntConfiguration<TypeBlock>
    {
        public override void Configure(EntityTypeBuilder<TypeBlock> builder)
        {
            base.Configure(builder);
            builder.ToTable("TypeBlock");

            builder.Property(c => c.Code).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired(true);
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
        }
    }
}
