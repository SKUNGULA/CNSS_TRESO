using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public sealed class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IAuditTrailService _audit;

    public ExchangeRateService(IExchangeRateRepository repository, ICurrencyRepository currencyRepository, IAuditTrailService audit)
    {
        _repository = repository;
        _currencyRepository = currencyRepository;
        _audit = audit;
    }

    public async Task<(bool IsSuccess, string? Error)> CreateExchangeRateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        if (exchangeRate is null)
        {
            return (false, "Le taux est obligatoire.");
        }

        if (await _repository.ExistsAsync(exchangeRate.RateDate, exchangeRate.CurrencyCode, cancellationToken: cancellationToken))
            return (false, "Un taux existe déjà pour cette date et cette devise.");

        await _repository.AddAsync(exchangeRate, cancellationToken);
        await _audit.RecordAsync("system", "ExchangeRate.Create", "ExchangeRate", exchangeRate.RateDate.ToString("yyyy-MM-dd"), $"{exchangeRate.CurrencyCode} = {exchangeRate.Amount}", cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> UpdateExchangeRateAsync(int number, DateOnly rateDate, string currencyCode, decimal amount, float percent, CancellationToken cancellationToken = default)
    {
        var rate = await _repository.GetByNumberAsync(number, cancellationToken);
        if (rate is null) return (false, "Taux introuvable.");
        if (await _repository.ExistsAsync(rateDate, currencyCode, number, cancellationToken)) return (false, "Un taux existe déjà pour cette date et cette devise.");

        rate.Update(rateDate, currencyCode, amount, percent);
        await _repository.UpdateAsync(rate, cancellationToken);
        await _audit.RecordAsync("system", "ExchangeRate.Update", "ExchangeRate", number.ToString(), $"{rate.CurrencyCode} = {rate.Amount}", cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> DeleteExchangeRateAsync(int number, CancellationToken cancellationToken = default)
    {
        var rate = await _repository.GetByNumberAsync(number, cancellationToken);
        if (rate is null) return (false, "Taux introuvable.");
        await _repository.DeleteAsync(number, cancellationToken);
        await _audit.RecordAsync("system", "ExchangeRate.Delete", "ExchangeRate", number.ToString(), $"{rate.CurrencyCode} {rate.RateDate:yyyy-MM-dd}", cancellationToken);
        return (true, null);
    }

    public Task<IReadOnlyList<ExchangeRate>> ListExchangeRatesAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(cancellationToken);

    public Task<IReadOnlyList<ExchangeRate>> ListRecentAsync(int take, string? currencyCode = null, CancellationToken cancellationToken = default)
        => _repository.GetRecentAsync(take, currencyCode, cancellationToken);

    public Task<IReadOnlyList<Currency>> ListCurrenciesAsync(CancellationToken cancellationToken = default)
        => _currencyRepository.GetAllAsync(cancellationToken);
}
