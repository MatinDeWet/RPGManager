using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class CampaignMemberConfig : IEntityTypeConfiguration<CampaignMember>
{
    public void Configure(EntityTypeBuilder<CampaignMember> entity)
    {
        entity.ToTable(nameof(CampaignMember), SchemaConstants.Default);

        // Composite key: a user holds at most one role per campaign.
        entity.HasKey(x => new { x.CampaignId, x.UserId });

        entity.Property(x => x.Role)
            .IsRequired();

        entity.HasOne(x => x.User)
            .WithMany(x => x.CampaignMemberships)
            .HasForeignKey(x => x.UserId);
    }
}
