using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Entities;
using Shared.Persistence.Constants;

namespace Shared.Persistence.Configuration;

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable(nameof(User), SchemaConstants.Default);
        
        entity.HasKey(x => x.Id);
        
        entity.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        entity.Property(x => x.IdentityId)
            .HasMaxLength(256)
            .IsRequired();

        entity.HasIndex(x => x.IdentityId)
            .IsUnique();

        entity.HasMany(x => x.Worlds)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        entity.HasMany(x => x.CampaignMemberships)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        entity.HasMany(x => x.IssuedInvitations)
            .WithOne(x => x.IssuedBy)
            .HasForeignKey(x => x.IssuedByUserId);

        entity.HasMany(x => x.AcceptedInvitations)
            .WithOne(x => x.AcceptedBy)
            .HasForeignKey(x => x.AcceptedByUserId);
    }
}
