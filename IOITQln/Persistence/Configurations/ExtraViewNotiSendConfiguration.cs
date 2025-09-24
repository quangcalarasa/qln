using IOITQln.Common.Bases.Configurations;
using IOITQln.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOITQln.Persistence.Configurations
{
    public class ExtraViewNotiSendConfiguration : AppEntityTypeIntConfiguration<ExtraViewNotiSend>
    {
        public override void Configure(EntityTypeBuilder<ExtraViewNotiSend> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExtraViewNotiSend");
        }
    }
}
