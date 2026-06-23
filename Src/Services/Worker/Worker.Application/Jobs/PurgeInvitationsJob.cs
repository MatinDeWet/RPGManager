using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Worker.Application.Logging;
using Worker.Application.Options;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace Worker.Application.Jobs;

internal sealed class PurgeInvitationsJob(
    ICampaignInvitationUnsecuredQueryRepo queryRepo,
    IOptions<InvitationPurgeOptions> options,
    ILogger<PurgeInvitationsJob> logger) : IPurgeInvitationsJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-options.Value.TerminalRetentionDays);

        int purged = await queryRepo.Invitations
            .Where(IsPurgeable(cutoff))
            .ExecuteDeleteAsync(cancellationToken);

        logger.PurgedInvitations(purged);
    }

    /// <summary>
    /// An invitation is purgeable once it has been inactive for longer than the retention window. The
    /// window is measured from when it became inactive — <see cref="CampaignInvitation.RespondedAt"/>
    /// for a terminal (accepted/declined/revoked) invitation, or <see cref="CampaignInvitation.ExpiresAt"/>
    /// for one that lapsed while still pending — not from its creation, so a long-lived invitation that
    /// was only just resolved still gets its full retention grace.
    /// </summary>
    internal static Expression<Func<CampaignInvitation, bool>> IsPurgeable(DateTimeOffset cutoff)
    {
        return x =>
            x.Status != InvitationStatus.Pending && x.RespondedAt != null && x.RespondedAt <= cutoff
            || x.Status == InvitationStatus.Pending && x.ExpiresAt <= cutoff;
    }
}
