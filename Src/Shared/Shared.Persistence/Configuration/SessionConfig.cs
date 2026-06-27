using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class SessionConfig : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> entity)
    {
        entity.ToTable(nameof(Session), SchemaConstants.Default);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.SessionNumber)
            .IsRequired();

        entity.Property(x => x.Title)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.ScheduledAt)
            .IsRequired();

        entity.Property(x => x.Summary)
            .HasMaxLength(4096);

        entity.Property(x => x.Status)
            .IsRequired();

        entity.HasOne(x => x.Campaign)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.CampaignId);

        entity.HasIndex(x => new { x.CampaignId, x.SessionNumber })
            .IsUnique();

        entity.HasIndex(x => x.ScheduledAt);
    }
}
