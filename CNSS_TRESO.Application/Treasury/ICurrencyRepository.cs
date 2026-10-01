using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
}
