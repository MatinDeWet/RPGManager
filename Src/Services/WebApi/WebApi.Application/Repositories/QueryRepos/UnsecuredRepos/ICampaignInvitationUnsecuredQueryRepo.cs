using Repository.Contracts;
using Shared.Domain.Entities;

namespace WebApi.Application.Repositories.QueryRepos.UnsecuredRepos;

public interface ICampaignInvitationUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<CampaignInvitation> Invitations { get; }
}
