using System.Security.Claims;
using CNSS_TRESO.Application.Security;
using CNSS_TRESO.Common;

namespace CNSS_TRESO.Security;

public sealed class HttpCallerContextAccessor : ICallerContextAccessor
{
    private readonly IHttpContextAccessor _http;

    public HttpCallerContextAccessor(IHttpContextAccessor http) => _http = http;

    public CallerContext? Current
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            var username = user.Identity.Name ?? string.Empty;
            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "Agent";
            var cf = user.FindFirst(ClaimTypesExt.CostCenter)?.Value ?? string.Empty;
            var mat = user.FindFirst(ClaimTypesExt.Matricule)?.Value ?? string.Empty;
            int.TryParse(user.FindFirst(ClaimTypesExt.Exercise)?.Value, out var exer);
            var db = user.FindFirst(ClaimTypesExt.DatabaseSuffix)?.Value ?? string.Empty;
            return new CallerContext(username, role, cf, mat, exer, db);
        }
    }
}
