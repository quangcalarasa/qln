using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.sistence.Configurations
{
    public class Md167DelegateConfiguration : AppEntityTypeIntConfiguration<Md167Delegate>
    {
        public override void Configure(EntityTypeBuilder<Md167Delegate> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167Delegate");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false).HasDefaultValue("0");
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.AutInfo).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.NationalId).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.PhoneNumber).HasMaxLength(1000).IsRequired();
            builder.Property(c => c.DateOfIssue).IsRequired(false);
            builder.Property(c => c.PlaceOfIssue).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ComTaxNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ComOrganizationRepresentativeName).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ComPosition).HasMaxLength(1000).IsRequired(false);
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.onalAreaValue).IsRequired(true);
        }
    }
}
