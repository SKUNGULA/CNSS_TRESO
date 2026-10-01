using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Treasury.Reporting;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Audit;

[Authorize(Policy = Policies.AdministratorOnly)]
public class IndexModel : PageModel
{
    private readonly IAuditTrailService _auditTrailService;
    private readonly IExcelExporter _exporter;

    public IndexModel(IAuditTrailService auditTrailService, IExcelExporter exporter)
    {
        _auditTrailService = auditTrailService;
        _exporter = exporter;
    }

    [BindProperty(SupportsGet = true)] public string? UsernameFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? ActionFilter { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int Max { get; set; } = 500;

    public IReadOnlyList<AuditEntry> Entries { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Entries = await _auditTrailService.SearchAsync(BuildFilter(), cancellationToken);
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        var filter = BuildFilter();
        var entries = await _auditTrailService.SearchAsync(filter, cancellationToken);
        var bytes = _exporter.ExportAuditEntries(entries, filter.Username, filter.Action, filter.FromUtc, filter.ToUtc);
        var name = $"audit_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
    }

    private AuditFilter BuildFilter()
    {
        DateTime? from = FromDate.HasValue ? DateTime.SpecifyKind(FromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? to = ToDate.HasValue ? DateTime.SpecifyKind(ToDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
        var max = Max <= 0 ? 500 : Math.Min(Max, 5000);
        return new AuditFilter(UsernameFilter, ActionFilter, from, to, max);
    }
}
