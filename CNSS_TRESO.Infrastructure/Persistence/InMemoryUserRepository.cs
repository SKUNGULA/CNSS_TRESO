using System.Collections.Concurrent;
using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Domain.Entities;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = _users.Values.ToList();
        return Task.FromResult(users);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (!_users.TryAdd(user.Username, user))
        {
            throw new InvalidOperationException($"User '{user.Username}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (!_users.ContainsKey(user.Username))
            throw new InvalidOperationException($"User '{user.Username}' does not exist.");

        _users[user.Username] = user;
        return Task.CompletedTask;
    }
}
