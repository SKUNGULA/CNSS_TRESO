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
public class DeleteModel : PageModel
{
    private readonly ITreasuryMovementService _movementService;

    public DeleteModel(ITreasuryMovementService movementService)
    {
        _movementService = movementService;
    }

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; }

    [BindProperty]
    public string Reason { get; set; } = string.Empty;

    public TreasuryMovement? Movement { get; private set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Movement = await _movementService.GetByNumberAsync(Number, cancellationToken);
        if (Movement is null) return NotFound();
        if (!Movement.IsDraft) ErrorMessage = "Seul un mouvement brouillon peut etre supprime.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Movement = await _movementService.GetByNumberAsync(Number, cancellationToken);
        if (Movement is null) return NotFound();

        var caller = BuildCaller();
        var result = await _movementService.DeleteDraftAsync(Number, Reason ?? string.Empty, caller, cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }
        return RedirectToPage("Index", new { SuccessMessage = $"Mouvement {Number} supprime." });
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
