using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class TokenConfiguration : AppEntityTypeGuidConfiguration<Token>
    {
        public override void Configure(EntityTypeBuilder<Token> builder)
        {
            base.Configure(builder);
            builder.ToTable("Token");

            builder.Property(c => c.AccessToken).HasColumnType("ntext").IsRequired(true);
        }
    }
}
