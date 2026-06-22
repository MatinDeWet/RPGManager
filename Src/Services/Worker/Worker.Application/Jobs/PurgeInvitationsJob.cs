using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Worker.Application.Logging;
using Worker.Application.Options;
using Worker.Application.Repositories.CommandRepos.UnsecuredRepos;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace Worker.Application.Jobs;

internal sealed class PurgeInvitationsJob(
    ICampaignInvitationUnsecuredQueryRepo queryRepo,
    IUnsecuredCommandRepo commandRepo,
    IOptions<InvitationPurgeOptions> options,
    ILogger<PurgeInvitationsJob> logger) : IPurgeInvitationsJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset cutoff = now.AddDays(-options.Value.TerminalRetentionDays);

        // Terminal (no longer pending) or expired invitations that were created longer ago than the
        // retention window. Pending-and-unexpired invitations are always kept.
        List<CampaignInvitation> stale = await queryRepo.Invitations
            .Where(x => (x.Status != InvitationStatus.Pending || x.ExpiresAt <= now) && x.DateCreated <= cutoff)
            .ToListAsync(cancellationToken);

        if (stale.Count == 0)
        {
            logger.PurgedInvitations(0);
            return;
        }

        foreach (CampaignInvitation invitation in stale)
        {
            await commandRepo.DeleteAsync(invitation, cancellationToken);
        }

        await commandRepo.SaveAsync(cancellationToken);

        logger.PurgedInvitations(stale.Count);
    }
}
