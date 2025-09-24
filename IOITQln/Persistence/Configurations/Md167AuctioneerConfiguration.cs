using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167AuctioneerConfiguration : AppEntityTypeIntConfiguration<Md167Auctioneer>
    {
        public override void Configure(EntityTypeBuilder<Md167Auctioneer> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Auctioneer");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.UnitAddress).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.AutInfo).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.TaxNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.BusinessLicense).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.RepresentFullName).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.RepresentPosition).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.RepresentIDCard).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.RepresentPlaceOfIssue).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ContactPhoneNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ContactAddress).HasMaxLength(1000).IsRequired(false);
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
