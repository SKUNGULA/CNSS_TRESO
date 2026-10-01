using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfExchangeRateRepository : IExchangeRateRepository
{
    private readonly CnssTresoDbContext _dbContext;

    public EfExchangeRateRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        _dbContext.ExchangeRates.Add(exchangeRate);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ExchangeRate?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
        => _dbContext.ExchangeRates.FirstOrDefaultAsync(rate => rate.Number == number, cancellationToken);

    public Task<bool> ExistsAsync(DateOnly rateDate, string currencyCode, int? excludingNumber = null, CancellationToken cancellationToken = default)
    {
        var code = currencyCode.Trim().ToUpperInvariant();
        return _dbContext.ExchangeRates.AnyAsync(rate => rate.RateDate == rateDate && rate.CurrencyCode == code && (!excludingNumber.HasValue || rate.Number != excludingNumber.Value), cancellationToken);
    }

    public async Task UpdateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        _dbContext.ExchangeRates.Update(exchangeRate);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int number, CancellationToken cancellationToken = default)
    {
        var rate = await _dbContext.ExchangeRates.FirstOrDefaultAsync(item => item.Number == number, cancellationToken);
        if (rate is null) return;
        _dbContext.ExchangeRates.Remove(rate);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExchangeRates
            .AsNoTracking()
            .OrderByDescending(x => x.RateDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetRecentAsync(int take, string? currencyCode = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ExchangeRates.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(currencyCode))
        {
            var code = currencyCode.Trim().ToUpperInvariant();
            query = query.Where(x => x.CurrencyCode == code);
        }
        return await query
            .OrderByDescending(x => x.RateDate)
            .ThenByDescending(x => x.Number)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal?> GetAmountAsync(string currencyCode, DateOnly date, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) return null;
        var code = currencyCode.Trim().ToUpperInvariant();
        var exact = await _dbContext.ExchangeRates.AsNoTracking()
            .Where(x => x.CurrencyCode == code && x.RateDate == date)
            .OrderByDescending(x => x.Number)
            .Select(x => x.Amount)
            .FirstOrDefaultAsync(cancellationToken);
        if (exact.HasValue && exact.Value > 0) return exact;
        return await _dbContext.ExchangeRates.AsNoTracking()
            .Where(x => x.CurrencyCode == code && x.RateDate <= date)
            .OrderByDescending(x => x.RateDate)
            .ThenByDescending(x => x.Number)
            .Select(x => x.Amount)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
