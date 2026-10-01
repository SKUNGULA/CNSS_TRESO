using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface IExchangeRateRepository
{
    Task AddAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(DateOnly rateDate, string currencyCode, int? excludingNumber = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
    Task DeleteAsync(int number, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> GetRecentAsync(int take, string? currencyCode = null, CancellationToken cancellationToken = default);
    Task<decimal?> GetAmountAsync(string currencyCode, DateOnly date, CancellationToken cancellationToken = default);
}
