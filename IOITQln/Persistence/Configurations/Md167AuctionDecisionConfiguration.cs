using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167AuctionDecisionConfiguration : AppEntityTypeLongConfiguration<Md167AuctionDecision>
    {
        public override void Configure(EntityTypeBuilder<Md167AuctionDecision> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167AuctionDecision");

            builder.Property(c => c.Decision).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
        }
    }
}
