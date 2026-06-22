using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.SecuredRepos;

public interface ICampaignInvitationSecuredQueryRepo : ISecureQueryRepo
{
    IQueryable<CampaignInvitation> Invitations { get; }
}
