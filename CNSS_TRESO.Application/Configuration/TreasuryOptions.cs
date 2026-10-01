namespace CNSS_TRESO.Application.Configuration;

public sealed class TreasuryOptions
{
    public int ExerciseYear { get; set; } = DateTime.UtcNow.Year;
}
