using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Common.Bases.Configurations
{
    public class AppEntityTypeLongConfiguration<T> : IEntityTypeConfiguration<T> where T : AbstractEntity<long>
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("getdate()");
            builder.Property(i => i.UpdatedAt).HasDefaultValueSql("getdate()");
            builder.Property(i => i.CreatedById).IsRequired(false);
            builder.Property(i => i.UpdatedById).IsRequired(false);
            builder.Property(i => i.CreatedBy).HasMaxLength(500).IsRequired(false);
            builder.Property(i => i.UpdatedBy).HasMaxLength(500).IsRequired(false);
            builder.Property(i => i.Status).HasDefaultValue(EntityStatus.NORMAL);
        }
    }
}
