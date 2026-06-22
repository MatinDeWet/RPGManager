using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class CampaignInvitationConfig : IEntityTypeConfiguration<CampaignInvitation>
{
    public void Configure(EntityTypeBuilder<CampaignInvitation> entity)
    {
        entity.ToTable(nameof(CampaignInvitation), SchemaConstants.Default);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.InviteeEmail)
            .HasMaxLength(256)
            .IsRequired();

        entity.Property(x => x.TokenHash)
            .IsRequired();

        entity.Property(x => x.IssuedByUserId)
            .IsRequired();

        entity.Property(x => x.ExpiresAt)
            .IsRequired();

        entity.Property(x => x.Status)
            .IsRequired();

        entity.HasOne(x => x.Campaign)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.CampaignId);

        entity.HasOne(x => x.IssuedBy)
            .WithMany(x => x.IssuedInvitations)
            .HasForeignKey(x => x.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.AcceptedBy)
            .WithMany(x => x.AcceptedInvitations)
            .HasForeignKey(x => x.AcceptedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => x.TokenHash)
            .IsUnique();

        entity.HasIndex(x => new { x.CampaignId, x.InviteeEmail })
            .IsUnique()
            .HasFilter($"\"{nameof(CampaignInvitation.Status)}\" = {(int)InvitationStatus.Pending}");

        entity.HasIndex(x => x.ExpiresAt);
    }
}
