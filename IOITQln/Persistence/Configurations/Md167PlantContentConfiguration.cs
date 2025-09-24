using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167PlantContentConfiguration : AppEntityTypeIntConfiguration<Md167PlantContent>
    {
        public override void Configure(EntityTypeBuilder<Md167PlantContent> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167PlantContent");

            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(2000).IsRequired(false);
            
        }
    }
}
