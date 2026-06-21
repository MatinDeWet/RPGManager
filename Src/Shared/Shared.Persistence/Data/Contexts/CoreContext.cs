using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Data.Contexts;

public class CoreContext : DbContext
{
    public CoreContext()
    {
        
    }
    
    public CoreContext(DbContextOptions<CoreContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Trigram extension backing case-insensitive ILIKE '%...%' substring searches.
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreContext).Assembly);
    }
}
