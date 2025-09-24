using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167StateOfUseConfiguration : AppEntityTypeIntConfiguration<Md167StateOfUse>
    {
        public override void Configure(EntityTypeBuilder<Md167StateOfUse> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167StateOfUse");

            builder.Property(c => c.Code).HasMaxLength(200).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            
        }
    }
}
