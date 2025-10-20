using Infrastructure.Persistence.DataContext.AppDb;
using Infrastructure.Persistence.DataContext.AppIdentityDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.Seeds;

public static class DbMigrator
{
    public static async Task MigrateAsync(IServiceProvider sp)
    {
        var appDbContext = sp.GetRequiredService<AppDbContext>();
        var identityDbContext = sp.GetRequiredService<AppIdentityDbContext>();
        
        await appDbContext.Database.MigrateAsync();
        await identityDbContext.Database.MigrateAsync();
    }
}