using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfPermissionRepository : IPermissionRepository
{
    private readonly CnssTresoDbContext _dbContext;

    public EfPermissionRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Module>> GetAllModulesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Module>()
            .AsNoTracking()
            .OrderBy(m => m.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserModuleAccess>> GetUserAccessAsync(string username, CancellationToken cancellationToken = default)
    {
        var u = (username ?? string.Empty).Trim();
        return await _dbContext.Set<UserModuleAccess>()
            .AsNoTracking()
            .Where(x => x.Username == u)
            .OrderBy(x => x.ModuleNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task GrantAsync(string username, int moduleNumber, CancellationToken cancellationToken = default)
    {
        var u = (username ?? string.Empty).Trim();
        var exists = await _dbContext.Set<UserModuleAccess>()
            .AnyAsync(x => x.Username == u && x.ModuleNumber == moduleNumber, cancellationToken);
        if (exists) return;

        _dbContext.Set<UserModuleAccess>().Add(new UserModuleAccess(u, moduleNumber));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(string username, int moduleNumber, CancellationToken cancellationToken = default)
    {
        var u = (username ?? string.Empty).Trim();
        var entity = await _dbContext.Set<UserModuleAccess>()
            .FirstOrDefaultAsync(x => x.Username == u && x.ModuleNumber == moduleNumber, cancellationToken);
        if (entity is null) return;

        _dbContext.Set<UserModuleAccess>().Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
