using Repository.Implementation;
using Shared.Persistence.Data.Contexts;
using Worker.Application.Repositories.CommandRepos.UnsecuredRepos;

namespace Worker.infrastructure.Repositories.CommandRepos.UnsecuredRepos;

internal sealed class UnsecuredCommandRepo(CoreContext context)
    : CommandRepo<CoreContext>(context), IUnsecuredCommandRepo;
