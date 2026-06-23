using Repository.Contracts;
using Shared.Domain.Entities;

namespace Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

public interface ICampaignInvitationUnsecuredQueryRepo : IQueryRepo
{
    IQueryable<CampaignInvitation> Invitations { get; }
}
