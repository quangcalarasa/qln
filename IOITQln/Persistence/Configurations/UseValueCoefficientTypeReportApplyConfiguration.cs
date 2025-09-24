using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class UseValueCoefficientTypeReportApplyConfiguration : AppEntityTypeIntConfiguration<UseValueCoefficientTypeReportApply>
    {
        public override void Configure(EntityTypeBuilder<UseValueCoefficientTypeReportApply> builder)
        {
            base.Configure(builder);
            builder.ToTable("UseValueCoefficientTypeReportApply");

        }
    }
}
