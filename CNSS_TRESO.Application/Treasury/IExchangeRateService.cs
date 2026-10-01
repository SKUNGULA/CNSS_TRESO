using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface IExchangeRateService
{
    Task<(bool IsSuccess, string? Error)> CreateExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> UpdateExchangeRateAsync(int number, DateOnly rateDate, string currencyCode, decimal amount, float percent, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? Error)> DeleteExchangeRateAsync(int number, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> ListExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExchangeRate>> ListRecentAsync(int take, string? currencyCode = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> ListCurrenciesAsync(CancellationToken cancellationToken = default);
}
