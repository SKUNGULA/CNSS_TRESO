using CNSS_TRESO.Application.Exercises;
using CNSS_TRESO.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace CNSS_TRESO.Infrastructure.Persistence;

public interface IExerciseDbContextFactory
{
    ExerciseDbContext Create();
}

public sealed class ExerciseDbContextFactory : IExerciseDbContextFactory
{
    private readonly ICallerContextAccessor _accessor;
    private readonly IExerciseConnectionStringBuilder _connectionStringBuilder;

    public ExerciseDbContextFactory(ICallerContextAccessor accessor, IExerciseConnectionStringBuilder connectionStringBuilder)
    {
        _accessor = accessor;
        _connectionStringBuilder = connectionStringBuilder;
    }

    public ExerciseDbContext Create()
    {
        var suffix = _accessor.Current?.DatabaseSuffix ?? string.Empty;
        var cs = _connectionStringBuilder.BuildFor(suffix);
        var options = new DbContextOptionsBuilder<ExerciseDbContext>()
            .UseSqlServer(cs)
            .Options;
        return new ExerciseDbContext(options);
    }
}
