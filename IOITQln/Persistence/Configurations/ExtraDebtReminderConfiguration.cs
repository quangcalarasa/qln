using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraDebtReminderConfiguration : AppEntityTypeIntConfiguration<ExtraDebtReminder>
    {
        public override void Configure(EntityTypeBuilder<ExtraDebtReminder> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraDebtReminder");

            //builder.Property(c => c.TypeReportApply).IsRequired();
            //builder.Property(c => c.BlockId).IsRequired();
            builder.Property(c => c.Date).IsRequired();
            builder.Property(c => c.DebtRemindNumber).IsRequired(true);
            builder.Property(c => c.Times).IsRequired(true);
            builder.Property(c => c.Code).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.House).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Apartment).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Owner).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.SDT).HasMaxLength(2000).IsRequired(false);
            builder.Property(c => c.Content).HasMaxLength(2000).IsRequired(false);
            //builder.Property(c => c.AreaValue).IsRequired(true);
            //builder.Property(c => c.GeneralAreaValue).IsRequired(true);
            //builder.Property(c => c.PeronalAreaValue).IsRequired(true);
        }
    }
}
