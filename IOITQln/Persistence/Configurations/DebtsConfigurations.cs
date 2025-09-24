using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class DebtsConfigurations :  AppEntityTypeIntConfiguration<Debts>
    {
        public override void Configure(EntityTypeBuilder<Debts> builder)
        {
            base.Configure(builder);
            builder.ToTable("Debts");

            builder.Property(c => c.SurplusBalance).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Diff).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Total).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Paid).HasColumnType("decimal(18,2)");

        }
    }
}
