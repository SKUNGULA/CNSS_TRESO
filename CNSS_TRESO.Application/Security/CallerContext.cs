namespace CNSS_TRESO.Application.Security;

public sealed record CallerContext(
    string Username,
    string Role,
    string CostCenter,
    string Matricule = "",
    int Exercise = 0,
    string DatabaseSuffix = "")
{
    public bool IsAdministrator => string.Equals(Role, "Administrator", StringComparison.OrdinalIgnoreCase);
}
