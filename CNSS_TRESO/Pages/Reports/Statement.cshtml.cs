using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Application.Treasury.Reporting;
using CNSS_TRESO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CNSS_TRESO.Pages.Reports;

[Authorize]
public class StatementModel : PageModel
{
    private readonly IStatementReportService _reportService;
    private readonly IJournalRepository _journals;
    private readonly IUserJournalRepository _userJournals;

    public StatementModel(IStatementReportService reportService, IJournalRepository journals, IUserJournalRepository userJournals)
    {
        _reportService = reportService;
        _journals = journals;
        _userJournals = userJournals;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime FromDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [BindProperty(SupportsGet = true)]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [BindProperty(SupportsGet = true)]
    public string? JournalCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CostCenter { get; set; }

    public IReadOnlyList<SelectListItem> JournalOptions { get; private set; } = Array.Empty<SelectListItem>();
    public StatementReport? Report { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadJournalsAsync(cancellationToken);
        CostCenter = UserScope.ResolveCostCenter(User, CostCenter);
        var allowedJournals = await GetAuthorizedJournalsAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(JournalCode) && !allowedJournals.Any(journal => string.Equals(journal.Number, JournalCode.Trim(), StringComparison.OrdinalIgnoreCase))) return Forbid();

        Report = await _reportService.GenerateAsync(
            DateOnly.FromDateTime(FromDate),
            DateOnly.FromDateTime(ToDate),
            JournalCode,
            CostCenter,
            allowedJournals.Select(journal => journal.Number).ToArray(),
            cancellationToken);
            return Page();
    }

    private async Task LoadJournalsAsync(CancellationToken cancellationToken)
    {
        var list = await GetAuthorizedJournalsAsync(cancellationToken);
        JournalOptions = list
            .Select(j => new SelectListItem
            {
                Value = j.Number,
                Text = $"[{j.JournalType}] {j.Number} — {j.Label}"
            })
            .ToList();
    }

    private async Task<IReadOnlyList<UserJournal>> GetAuthorizedJournalsAsync(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? string.Empty;
        var costCenter = User.FindFirstValue(ClaimTypesExt.CostCenter) ?? string.Empty;
        return await _userJournals.ListAsync(username, costCenter, cancellationToken);
    }
}
