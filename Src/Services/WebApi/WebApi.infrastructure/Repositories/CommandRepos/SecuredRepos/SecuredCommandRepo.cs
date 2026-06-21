using Identification.Contracts;
using Repository.Contracts;
using Repository.Implementation;
using WebApi.Application.Repositories.CommandRepos.SecuredRepos;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.CommandRepos.SecuredRepos;

internal sealed class SecuredCommandRepo(CoreContext context, IIdentityInfo info, IEnumerable<IProtected> protection)
    : SecureCommandRepo<CoreContext>(context, info, protection), ISecuredCommandRepo;
