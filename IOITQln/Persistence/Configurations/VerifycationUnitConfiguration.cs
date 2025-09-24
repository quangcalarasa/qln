using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class VerifycationUnitConfiguration : AppEntityTypeIntConfiguration<VerifycationUnit>
    {
        public override void Configure(EntityTypeBuilder<VerifycationUnit> builder)
        {
            base.Configure(builder);
            builder.ToTable("VerifycationUnit");

            builder.Property(c => c.Code).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(4000).IsRequired(false);
        }
    }
}
