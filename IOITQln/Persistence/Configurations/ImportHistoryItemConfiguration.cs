using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace IOITQln.Persistence.Configurations
{
    public class ImportHistoryItemConfiguration : AppEntityTypeGuidConfiguration<ImportHistoryItem>
    {
        public override void Configure(EntityTypeBuilder<ImportHistoryItem> builder)
        {
            base.Configure(builder);
            builder.ToTable("ImportHistoryItem");

            builder.Property(c => c.Id).HasDefaultValue(new Guid());
            builder.Property(c => c.DataStorage).HasColumnType("ntext");
        }
    }
}
