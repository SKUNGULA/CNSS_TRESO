using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfCurrencyRepository : ICurrencyRepository
{
    private readonly CnssTresoDbContext _dbContext;

    public EfCurrencyRepository(CnssTresoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }
}
