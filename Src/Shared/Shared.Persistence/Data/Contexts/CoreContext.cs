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

        // Trigram extension backing the case-insensitive ILIKE '%...%' searches (e.g. travel-history file names).
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreContext).Assembly);
    }
}
