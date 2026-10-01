using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.DTOs;
using CNSS_TRESO.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CNSS_TRESO.Infrastructure.DependencyInjection;

public sealed class UserSeedHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public UserSeedHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        await users.CreateUserAsync(new CreateUserRequest(
            Username: "admin",
            FullName: "Administrateur CNSS",
            PlainPassword: "Admin@123",
            AccessLevel: AccessLevel.Administrator,
            CostCenter: "CNSS"
        ), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
