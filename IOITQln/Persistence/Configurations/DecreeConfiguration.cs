using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DecreeConfiguration : AppEntityTypeIntConfiguration<Decree>
    {
        public override void Configure(EntityTypeBuilder<Decree> builder)
        {
            base.Configure(builder);
            builder.ToTable("Decree");

            builder.Property(c => c.TypeDecree).IsRequired(true);
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.DoPub).IsRequired(false);
            builder.Property(c => c.DecisionUnit).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.ApplyCalculateRentalPrice).IsRequired(false);
        }
    }
}
