using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Common.Bases.Configurations
{
    public class AppEntityTypeBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity<int>
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(i => i.Status).HasDefaultValue(EntityStatus.NORMAL);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("getdate()");
            builder.Property(i => i.UpdatedAt).HasDefaultValueSql("getdate()");
        }
    }

    public class AppEntityTypeBaseLongConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity<long>
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(i => i.Status).HasDefaultValue(EntityStatus.NORMAL);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("getdate()");
            builder.Property(i => i.UpdatedAt).HasDefaultValueSql("getdate()");
        }
    }

    public class AppEntityTypeBaseGuidConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity<Guid>
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.Property(i => i.Status).HasDefaultValue(EntityStatus.NORMAL);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("getdate()");
            builder.Property(i => i.UpdatedAt).HasDefaultValueSql("getdate()");
        }
    }
}
