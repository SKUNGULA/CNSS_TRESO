using CNSS_TRESO.Application.Security;
using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CNSS_TRESO.Pages.Treasury.Movements;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITreasuryMovementService _movementService;
    private readonly IUserJournalRepository _journalRepository;

    public IndexModel(ITreasuryMovementService movementService, IUserJournalRepository journalRepository)
    {
        _movementService = movementService;
        _journalRepository = journalRepository;
    }

    public IReadOnlyList<TreasuryMovement> Movements { get; private set; } = [];
    public IReadOnlyList<UserJournal> Journals { get; private set; } = [];
    public string? ErrorMessage { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? SuccessMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? Journal { get; set; }
    [BindProperty(SupportsGet = true)]
    public bool OnlyUnvalidated { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostValidateAsync(int number, CancellationToken cancellationToken)
    {
        var result = await _movementService.ValidateAsync(number, BuildCaller(), cancellationToken);
        if (!result.IsSuccess) ErrorMessage = result.Error; else SuccessMessage = $"Mouvement {number} validé.";
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDevalidateAsync(int number, string reason, CancellationToken cancellationToken)
    {
        var result = await _movementService.DevalidateAsync(number, reason, BuildCaller(), cancellationToken);
        if (!result.IsSuccess) ErrorMessage = result.Error; else SuccessMessage = $"Mouvement {number} dévalidé.";
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostValidateGroupAsync(string journal, DateTime movementDate, CancellationToken cancellationToken)
    {
        var result = await _movementService.ValidateGroupAsync(journal, DateOnly.FromDateTime(movementDate), BuildCaller(), cancellationToken);
        if (!result.IsSuccess) ErrorMessage = result.Error; else SuccessMessage = result.Error;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDevalidateGroupAsync(string journal, DateTime movementDate, string reason, CancellationToken cancellationToken)
    {
        var result = await _movementService.DevalidateGroupAsync(journal, DateOnly.FromDateTime(movementDate), reason, BuildCaller(), cancellationToken);
        if (!result.IsSuccess) ErrorMessage = result.Error; else SuccessMessage = result.Error;
        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var costCenter = UserScope.ResolveCostCenter(User, null);
        var caller = BuildCaller();
        Journals = await _journalRepository.ListAsync(caller.Username, caller.CostCenter, cancellationToken);

        var filter = new MovementFilter(
            FromDate: FromDate.HasValue ? DateOnly.FromDateTime(FromDate.Value) : null,
            ToDate: ToDate.HasValue ? DateOnly.FromDateTime(ToDate.Value) : null,
            Journal: string.IsNullOrWhiteSpace(Journal) ? null : Journal.Trim(),
            CostCenter: costCenter,
            OnlyUnvalidated: OnlyUnvalidated ? true : null,
            Take: 300);
        Movements = await _movementService.SearchAsync(filter, cancellationToken);
    }

    private CallerContext BuildCaller()
    {
        var username = User.Identity?.Name ?? "anonymous";
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Agent";
        var costCenter = User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExt.CostCenter)?.Value ?? "";
        var matricule = User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExt.Matricule)?.Value ?? "";
        var exerciseStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExt.Exercise)?.Value ?? "0";
        int.TryParse(exerciseStr, out var exercise);
        var dbSuffix = User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExt.DatabaseSuffix)?.Value ?? "";
        return new CallerContext(username, role, costCenter, matricule, exercise, dbSuffix);
    }
}
