using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ImportHistoryConfiguration : AppEntityTypeLongConfiguration<ImportHistory>
    {
        public override void Configure(EntityTypeBuilder<ImportHistory> builder)
        {
            base.Configure(builder);
            builder.ToTable("ImportHistory");

            builder.Property(c => c.Type).IsRequired();
            builder.Property(c => c.FileUrl).HasMaxLength(2000).IsRequired();
            builder.Property(c => c.DataStorage).HasColumnType("ntext");
            builder.Property(c => c.DataExtraStorage).HasColumnType("ntext");
        }
    }
}
