using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class Md167HouseConfiguration : AppEntityTypeIntConfiguration<Md167House>
    {
        public override void Configure(EntityTypeBuilder<Md167House> builder)
        {
            base.Configure(builder);
            builder.ToTable("Md167House");

            builder.Property(c => c.Code).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.HouseNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ProvinceId).IsRequired();
            builder.Property(c => c.DistrictId).IsRequired();
            builder.Property(c => c.WardId).IsRequired();
            builder.Property(c => c.LandId).IsRequired();
            builder.Property(c => c.LaneId).IsRequired();
            builder.Property(c => c.PurposeUsing).IsRequired();
            builder.Property(c => c.TypeHouse).IsRequired();
            builder.Property(c => c.StatusOfUse).IsRequired();
            builder.Property(c => c.Note).IsRequired(false);
            builder.Property(c => c.MapNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ParcelNumber).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.LandTaxRate).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.LandTaxRate).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.OriginPrice).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.LocationCoefficientValue).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.UnitPriceValue).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.LandPrice).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.ValueLand).HasColumnType("decimal(18,5)").IsRequired();
            builder.Property(c => c.PlanningInfor).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Md167TransferUnitId).IsRequired();
            builder.Property(c => c.Location).IsRequired();
            builder.Property(c => c.LocationCoefficient).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.UnitPrice).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.SHNNCode).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.ContractCode).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.LeaseCode).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.LeaseCertCode).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Md167HouseId).IsRequired(false);
            builder.Property(c => c.DocumentCode).HasMaxLength(1000).IsRequired(false);
        }
    }
}
