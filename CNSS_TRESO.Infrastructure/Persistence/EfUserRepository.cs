using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfUserRepository : IUserRepository
{
    private readonly CnssTresoDbContext _dbContext;

    public EfUserRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        // Legacy : connexion possible par "Nom utilisateur" OU "Identifiant" (matricule)
        return _dbContext.Users
            .FirstOrDefaultAsync(x => x.Username == username || x.Matricule == username, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
