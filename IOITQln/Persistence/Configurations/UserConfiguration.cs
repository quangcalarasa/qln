using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class UserConfiguration : AppEntityTypeIntConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.ToTable("User");

            builder.Property(c => c.FullName).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.UserName).HasMaxLength(500).IsRequired();
            builder.Property(c => c.Password).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.Phone).HasMaxLength(100).IsRequired(false);
            builder.Property(c => c.Email).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Avatar).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.KeyLock).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.RegEmail).HasMaxLength(200).IsRequired(false);
            builder.Property(c => c.PasswordKeyCloak).HasMaxLength(500).IsRequired(false);
        }
    }
}
