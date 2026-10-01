using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Budget;
using CNSS_TRESO.Application.Configuration;
using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.Exercises;
using CNSS_TRESO.Application.Services;
using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Application.Treasury.Reporting;
using CNSS_TRESO.Infrastructure.Exercises;
using CNSS_TRESO.Infrastructure.Persistence;
using CNSS_TRESO.Infrastructure.Reporting;
using CNSS_TRESO.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CNSS_TRESO.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddCleanArchitectureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CnssTreso")
            ?? throw new InvalidOperationException("Connection string 'CnssTreso' is missing.");

        services.Configure<TreasuryOptions>(configuration.GetSection("Treasury"));
        services.Configure<ReportBranding>(configuration.GetSection("ReportBranding"));

        services.AddDbContext<CnssTresoDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IExerciseConnectionStringBuilder, ExerciseConnectionStringBuilder>();
        services.AddScoped<IExerciseDbContextFactory, ExerciseDbContextFactory>();

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserSecurityRepository, SqlUserSecurityRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddScoped<IExchangeRateRepository, EfExchangeRateRepository>();
        services.AddScoped<ICurrencyRepository, EfCurrencyRepository>();
        services.AddScoped<ITreasuryMovementRepository, EfTreasuryMovementRepository>();
        services.AddScoped<IDocumentRepository, EfDocumentRepository>();
        services.AddScoped<IJournalRepository, EfJournalRepository>();
        services.AddScoped<ISignatoryRepository, EfSignatoryRepository>();
        services.AddScoped<IAuditEntryRepository, EfAuditEntryRepository>();
        services.AddScoped<IPermissionRepository, EfPermissionRepository>();
        services.AddScoped<IBudgetExecutionRepository, SqlBudgetExecutionRepository>();
        services.AddScoped<ISuiviTpRepository, SqlSuiviTpRepository>();
        services.AddScoped<IComptaRepository, SqlComptaRepository>();
        services.AddScoped<IUserJournalRepository, SqlUserJournalRepository>();
        services.AddScoped<ICostCenterDirectory, SqlCostCenterDirectory>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ITreasuryMovementService, TreasuryMovementService>();
        services.AddScoped<IBudgetIntegrationService, BudgetIntegrationService>();
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<ICashReportService, CashReportService>();
        services.AddScoped<ITiersReportService, TiersReportService>();
        services.AddScoped<IStatementReportService, StatementReportService>();
        services.AddSingleton<IExcelExporter, ClosedXmlExcelExporter>();
        services.AddSingleton<IPdfExporter, QuestPdfExporter>();

        services.AddSingleton<IExerciseCatalog, SqlExerciseCatalog>();

        return services;
    }
}
