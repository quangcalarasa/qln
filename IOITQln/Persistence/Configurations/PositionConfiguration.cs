using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class PositionConfiguration : AppEntityTypeIntConfiguration<Position>
    {
        public override void Configure(EntityTypeBuilder<Position> builder)
        {
            base.Configure(builder);
            builder.ToTable("Position");

            builder.Property(c => c.Code).HasMaxLength(255).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.Note).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.AuthorDocsCode).HasMaxLength(2000).IsRequired(false);
        }
    }
}
