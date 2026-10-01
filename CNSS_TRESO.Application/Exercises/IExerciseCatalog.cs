namespace CNSS_TRESO.Application.Exercises;

public interface IExerciseCatalog
{
    Task<IReadOnlyList<ExerciseInfo>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed record ExerciseInfo(int Year, string DatabaseName)
{
    public string Suffix => DatabaseName.Length >= 2
        ? DatabaseName.Substring(DatabaseName.Length - 2)
        : DatabaseName;
}
