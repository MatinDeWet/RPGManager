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

    internal static Expression<Func<CampaignInvitation, bool>> IsPurgeable(DateTimeOffset cutoff)
    {
        return x =>
            x.Status != InvitationStatus.Pending && x.RespondedAt != null && x.RespondedAt <= cutoff
            || x.Status == InvitationStatus.Pending && x.ExpiresAt <= cutoff;
    }
}
