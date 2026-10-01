namespace CNSS_TRESO.Common;

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Username = "Username";
    public const string FullName = "FullName";
    public const string AccessLevel = "AccessLevel";
    public const string CostCenter = "CostCenter";
}

public static class ClaimTypesExt
{
    public const string CostCenter = "CostCenter";
    public const string Matricule = "Matricule";
    public const string Exercise = "Exercise";
    public const string DatabaseSuffix = "DatabaseSuffix";
}

public static class UserScope
{
    /// <summary>
    /// Centre de frais que l utilisateur peut effectivement interroger.
    /// Un administrateur peut viser un centre arbitraire (parametre requested).
    /// Un utilisateur non administrateur est toujours restreint a son propre centre de frais.
    /// </summary>
    public static string? ResolveCostCenter(System.Security.Claims.ClaimsPrincipal user, string? requested)
    {
        var own = user.FindFirst(ClaimTypesExt.CostCenter)?.Value;
        if (user.IsInRole("Administrator"))
            return string.IsNullOrWhiteSpace(requested) ? own : requested.Trim();
        return string.IsNullOrWhiteSpace(own) ? null : own;
    }
}
