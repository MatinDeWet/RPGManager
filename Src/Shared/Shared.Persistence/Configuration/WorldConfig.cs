using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class WorldConfig : IEntityTypeConfiguration<World>
{
    public void Configure(EntityTypeBuilder<World> entity)
    {
        entity.ToTable(nameof(World), SchemaConstants.Default);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(x => x.Description)
            .HasMaxLength(4096);

        entity.HasOne(x => x.User)
            .WithMany(x => x.Worlds)
            .HasForeignKey(x => x.UserId);
    }
}
