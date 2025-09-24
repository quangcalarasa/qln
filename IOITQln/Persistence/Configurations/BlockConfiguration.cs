using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class BlockConfiguration : AppEntityTypeIntConfiguration<Block>
    {
        public override void Configure(EntityTypeBuilder<Block> builder)
        {
            base.Configure(builder);
            builder.ToTable("Block");

            builder.Property(c => c.TypeReportApply).IsRequired();
            builder.Property(c => c.TypeBlockId).IsRequired();
            builder.Property(c => c.FloorApplyPriceChange).IsRequired();
            builder.Property(c => c.FloorBlockMap).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.LandNo).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.MapNo).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Width).IsRequired(false);
            builder.Property(c => c.Deep).IsRequired(false);
            builder.Property(c => c.Code).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.Name).HasMaxLength(1000).IsRequired(false);
            builder.Property(c => c.Address).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.Lane).IsRequired(false);
            builder.Property(c => c.Ward).IsRequired();
            builder.Property(c => c.District).IsRequired();
            builder.Property(c => c.Province).IsRequired();
            builder.Property(c => c.TypePile).IsRequired();
            builder.Property(c => c.ConstructionAreaNote).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue).IsRequired(true);
            builder.Property(c => c.ConstructionAreaValue1).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue2).IsRequired(false);
            builder.Property(c => c.ConstructionAreaValue3).IsRequired(false);
            builder.Property(c => c.UseAreaNote).HasMaxLength(4000).IsRequired(false);
            builder.Property(c => c.UseAreaValue).IsRequired(true);
            builder.Property(c => c.UseAreaValue1).IsRequired(false);
            builder.Property(c => c.UseAreaValue2).IsRequired(false);
            builder.Property(c => c.LandUsePlanningInfo).HasMaxLength(4000);
            builder.Property(c => c.HighwayPlanningInfo).HasMaxLength(4000);
            builder.Property(c => c.LandAcquisitionSituationInfo).HasMaxLength(4000);
            builder.Property(c => c.TextBasedInfo).HasMaxLength(4000);
            builder.Property(c => c.PositionCoefficientStr_99).HasMaxLength(100);
            builder.Property(c => c.PositionCoefficientStr_34).HasMaxLength(100);
            builder.Property(c => c.PositionCoefficientStr_61).HasMaxLength(100);
            builder.Property(c => c.AlleyPositionCoefficientStr_34).HasMaxLength(100);
            builder.Property(c => c.AlleyLandScapePrice_34).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandScapePrice_34).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandScapePrice_61).HasColumnType("decimal(18,2)");
            builder.Property(c => c.LandScapePrice_99).HasColumnType("decimal(18,2)");
            builder.Property(c => c.UsageStatusNote).HasMaxLength(4000);
            builder.Property(c => c.UseAreaNote1).HasMaxLength(4000);
            builder.Property(c => c.No2LandScapePrice_61).HasColumnType("decimal(18,2)");
            builder.Property(c => c.No).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.CodeEstablishStateOwnership).HasMaxLength(500).IsRequired(false);
            builder.Property(c => c.NameBlueprint).HasMaxLength(1000).IsRequired(false);
        }
    }
}
