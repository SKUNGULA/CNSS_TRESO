namespace CNSS_TRESO.Application.Exercises;

public interface IExerciseConnectionStringBuilder
{
    string BuildFor(string databaseSuffix);
}
