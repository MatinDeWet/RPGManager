using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class CampaignConfig : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> entity)
    {
        entity.ToTable(nameof(Campaign), SchemaConstants.Default);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        entity.HasIndex(x => x.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        entity.Property(x => x.Description)
            .HasMaxLength(4096);

        entity.HasOne(x => x.World)
            .WithMany(x => x.Campaigns)
            .HasForeignKey(x => x.WorldId);

        entity.HasMany(x => x.Members)
            .WithOne(x => x.Campaign)
            .HasForeignKey(x => x.CampaignId);
    }
}
