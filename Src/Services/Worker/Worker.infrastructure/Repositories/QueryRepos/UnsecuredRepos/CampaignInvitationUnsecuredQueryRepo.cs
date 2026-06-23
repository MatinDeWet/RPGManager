using Repository.Implementation;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace Worker.infrastructure.Repositories.QueryRepos.UnsecuredRepos;

internal sealed class CampaignInvitationUnsecuredQueryRepo(CoreContext context)
    : QueryRepo<CoreContext>(context), ICampaignInvitationUnsecuredQueryRepo
{
    public IQueryable<CampaignInvitation> Invitations => GetQueryable<CampaignInvitation>();
}
