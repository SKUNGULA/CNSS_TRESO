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
public class CashModel : PageModel
{
    private readonly ICashReportService _reportService;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;
    private readonly IJournalRepository _journals;
    private readonly IUserJournalRepository _userJournals;

    public CashModel(ICashReportService reportService, IExcelExporter excelExporter, IPdfExporter pdfExporter, IJournalRepository journals, IUserJournalRepository userJournals)
    {
        _reportService = reportService;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
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
    public CashReport? Report { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadJournalsAsync(cancellationToken);

        CostCenter = UserScope.ResolveCostCenter(User, CostCenter);

        if (!string.IsNullOrWhiteSpace(JournalCode))
        {
            if (!await IsJournalAuthorizedAsync(JournalCode, cancellationToken))
            {
                ModelState.AddModelError(nameof(JournalCode), "Vous n'êtes pas autorisé à consulter ce journal.");
                return;
            }
            Report = await _reportService.GenerateAsync(
                JournalCode!.Trim(),
                DateOnly.FromDateTime(FromDate),
                DateOnly.FromDateTime(ToDate),
                CostCenter,
                cancellationToken);
        }
    }

    public async Task<IActionResult> OnGetExcelAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(JournalCode))
        {
            await LoadJournalsAsync(cancellationToken);
            ModelState.AddModelError(nameof(JournalCode), "Veuillez choisir un journal.");
            return Page();
        }
        if (!await IsJournalAuthorizedAsync(JournalCode, cancellationToken)) return Forbid();

        CostCenter = UserScope.ResolveCostCenter(User, CostCenter);

        var report = await _reportService.GenerateAsync(
            JournalCode!.Trim(),
            DateOnly.FromDateTime(FromDate),
            DateOnly.FromDateTime(ToDate),
            CostCenter,
            cancellationToken);

        var bytes = _excelExporter.ExportCashReport(report);
        var kind = string.Equals(report.JournalType, "C", StringComparison.OrdinalIgnoreCase) ? "caisse" : "banque";
        var fileName = $"livre-{kind}_{report.JournalCode}_{report.FromDate:yyyyMMdd}_{report.ToDate:yyyyMMdd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> OnGetPdfAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(JournalCode))
        {
            await LoadJournalsAsync(cancellationToken);
            ModelState.AddModelError(nameof(JournalCode), "Veuillez choisir un journal.");
            return Page();
        }
        if (!await IsJournalAuthorizedAsync(JournalCode, cancellationToken)) return Forbid();

        CostCenter = UserScope.ResolveCostCenter(User, CostCenter);
        var report = await _reportService.GenerateAsync(
            JournalCode.Trim(),
            DateOnly.FromDateTime(FromDate),
            DateOnly.FromDateTime(ToDate),
            CostCenter,
            cancellationToken);

        var bytes = _pdfExporter.ExportCashReport(report);
        var kind = string.Equals(report.JournalType, "C", StringComparison.OrdinalIgnoreCase) ? "caisse" : "banque";
        var fileName = $"livre-{kind}_{report.JournalCode}_{report.FromDate:yyyyMMdd}_{report.ToDate:yyyyMMdd}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    private async Task LoadJournalsAsync(CancellationToken cancellationToken)
    {
        var list = await GetAuthorizedJournalsAsync(cancellationToken);
        JournalOptions = list
            .Where(j => j.JournalType == "C" || j.JournalType == "B")
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

    private async Task<bool> IsJournalAuthorizedAsync(string journal, CancellationToken cancellationToken) =>
        (await GetAuthorizedJournalsAsync(cancellationToken)).Any(item => string.Equals(item.Number, journal.Trim(), StringComparison.OrdinalIgnoreCase));
}
