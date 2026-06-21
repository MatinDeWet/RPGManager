using Repository.Implementation;
using WebApi.Application.Repositories.CommandRepos.UnsecuredRepos;
using Shared.Persistence.Data.Contexts;

namespace WebApi.infrastructure.Repositories.CommandRepos.UnsecuredRepos;

internal sealed class UnsecuredCommandRepo(CoreContext context)
    : CommandRepo<CoreContext>(context), IUnsecuredCommandRepo;
