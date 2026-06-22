using Repository.Contracts;
using Shared.Domain.Entities;

namespace Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

/// <summary>
/// Unsecured query repository for the worker's <see cref="CampaignInvitation"/> access. Background
/// jobs run with no current-user identity, so they read without any row-level security filtering.
/// </summary>
public interface ICampaignInvitationUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<CampaignInvitation> Invitations { get; }
}
