using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.DTOs;
using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Users;

[Authorize(Policy = Policies.AdministratorOnly)]
public class EditModel : PageModel
{
    private readonly IUserManagementService _userManagementService;
    private readonly ICostCenterDirectory _costCenterDirectory;

    public EditModel(IUserManagementService userManagementService, ICostCenterDirectory costCenterDirectory)
    {
        _userManagementService = userManagementService;
        _costCenterDirectory = costCenterDirectory;
    }

    [BindProperty(SupportsGet = true)]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyList<CostCenterOption> CostCenters { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCostCentersAsync(cancellationToken);
        var user = await _userManagementService.GetUserAsync(Username, cancellationToken);
        if (user is null) return NotFound();

        Input = new InputModel { Matricule = user.Matricule, AccessLevel = (int)user.AccessLevel, CostCenter = user.CostCenter };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadCostCentersAsync(cancellationToken);
        var result = await _userManagementService.UpdateUserAsync(
            new UpdateUserRequest(Username, Input.Matricule, Input.AccessLevel == 0 ? AccessLevel.Administrator : AccessLevel.Agent, Input.CostCenter),
            cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }
        return RedirectToPage("./Index", new { SuccessMessage = "Utilisateur modifié avec succès." });
    }

    private async Task LoadCostCentersAsync(CancellationToken cancellationToken) =>
        CostCenters = await _costCenterDirectory.ListAsync(cancellationToken);

    public sealed class InputModel
    {
        public string Matricule { get; set; } = string.Empty;
        public int AccessLevel { get; set; } = 1;
        public string CostCenter { get; set; } = string.Empty;
    }
}