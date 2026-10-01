using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Users;

[Authorize(Policy = Policies.AdministratorOnly)]
public class PermissionsModel : PageModel
{
    private readonly IPermissionService _permissionService;

    public PermissionsModel(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [BindProperty(SupportsGet = true)]
    public string Username { get; set; } = string.Empty;

    public IReadOnlyList<ModulePermissionView> Modules { get; private set; } = [];

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Username)) return NotFound();
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostGrantAsync(int moduleNumber, CancellationToken cancellationToken)
    {
        try
        {
            await _permissionService.GrantAsync(Username, moduleNumber, cancellationToken);
            SuccessMessage = $"Accès au module {moduleNumber} accordé.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRevokeAsync(int moduleNumber, CancellationToken cancellationToken)
    {
        try
        {
            await _permissionService.RevokeAsync(Username, moduleNumber, cancellationToken);
            SuccessMessage = $"Accès au module {moduleNumber} retiré.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }

        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Modules = await _permissionService.GetUserModulesAsync(Username, cancellationToken);
    }
}
