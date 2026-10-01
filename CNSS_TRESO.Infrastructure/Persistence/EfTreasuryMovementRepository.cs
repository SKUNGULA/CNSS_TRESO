using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public sealed class EfTreasuryMovementRepository : ITreasuryMovementRepository
{
    private readonly IExerciseDbContextFactory _factory;

    public EfTreasuryMovementRepository(IExerciseDbContextFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> AddAsync(TreasuryMovement movement, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        ctx.Movements.Add(movement);
        await ctx.SaveChangesAsync(cancellationToken);
        return movement.Number;
    }

    public async Task<IReadOnlyList<TreasuryMovement>> SearchAsync(MovementFilter filter, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        var q = ctx.Movements.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            q = q.Where(m => m.MovementDate >= filter.FromDate);
        if (filter.ToDate.HasValue)
            q = q.Where(m => m.MovementDate <= filter.ToDate);
        if (!string.IsNullOrWhiteSpace(filter.Journal))
            q = q.Where(m => m.Journal == filter.Journal);
        if (filter.JournalNumbers is { Count: > 0 })
            q = q.Where(m => filter.JournalNumbers.Contains(m.Journal));
        if (!string.IsNullOrWhiteSpace(filter.DocumentCode))
            q = q.Where(m => m.DocumentCode == filter.DocumentCode);
        if (!string.IsNullOrWhiteSpace(filter.CostCenter))
            q = q.Where(m => m.CostCenter == filter.CostCenter);
        if (!string.IsNullOrWhiteSpace(filter.Direction))
            q = q.Where(m => m.Direction == filter.Direction);
        if (filter.OnlyUnvalidated == true)
            q = q.Where(m => m.Validated != true);

        return await q
            .OrderByDescending(m => m.Number)
            .Take(filter.Take)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> SumAmountAsync(string? journal, string? costCenter, string direction, DateOnly upTo, bool strictlyBefore, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        var q = ctx.Movements.AsNoTracking().Where(m => m.Direction == direction);
        if (!string.IsNullOrWhiteSpace(journal)) q = q.Where(m => m.Journal == journal);
        if (!string.IsNullOrWhiteSpace(costCenter)) q = q.Where(m => m.CostCenter == costCenter);
        q = strictlyBefore ? q.Where(m => m.MovementDate < upTo) : q.Where(m => m.MovementDate <= upTo);
        return await q.SumAsync(m => (decimal?)m.Amount, cancellationToken) ?? 0m;
    }

    public async Task<TreasuryMovement?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        return await ctx.Movements.FirstOrDefaultAsync(m => m.Number == number, cancellationToken);
    }

    public async Task UpdateValidationAsync(int number, bool validated, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        var m = await ctx.Movements.FirstOrDefaultAsync(x => x.Number == number, cancellationToken);
        if (m is null) return;
        if (validated) m.MarkValidated(); else m.MarkUnvalidated();
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateValidationByJournalAndDateAsync(string journal, DateOnly movementDate, bool validated, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        await using var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);
        var movements = await ctx.Movements
            .Where(movement => movement.Journal == journal && movement.MovementDate == movementDate && movement.Validated != validated)
            .ToListAsync(cancellationToken);

        foreach (var movement in movements)
        {
            if (validated) movement.MarkValidated(); else movement.MarkUnvalidated();
        }

        await ctx.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return movements.Count;
    }

    public async Task UpdateDraftAsync(TreasuryMovement movement, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        ctx.Movements.Update(movement);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int number, CancellationToken cancellationToken = default)
    {
        await using var ctx = _factory.Create();
        var m = await ctx.Movements.FirstOrDefaultAsync(x => x.Number == number, cancellationToken);
        if (m is null) return;
        ctx.Movements.Remove(m);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}
