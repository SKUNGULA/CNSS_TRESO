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
public class IndexModel : PageModel
{
    private readonly IUserManagementService _userManagementService;
    private readonly ICostCenterDirectory _costCenterDirectory;

    public IndexModel(IUserManagementService userManagementService, ICostCenterDirectory costCenterDirectory)
    {
        _userManagementService = userManagementService;
        _costCenterDirectory = costCenterDirectory;
    }

    public IReadOnlyList<UserDto> Users { get; private set; } = [];
    public IReadOnlyList<CostCenterOption> CostCenters { get; private set; } = [];

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadUsersAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        var request = new CreateUserRequest(
            Input.Username,
            Input.FullName,
            Input.Password,
            Input.AccessLevel == 0 ? AccessLevel.Administrator : AccessLevel.Agent,
            Input.CostCenter
        );

        var result = await _userManagementService.CreateUserAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            await LoadUsersAsync(cancellationToken);
            return Page();
        }

        SuccessMessage = "Utilisateur cree avec succes.";
        Input = new CreateUserInput();
        await LoadUsersAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSetActiveAsync(string username, bool active, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.SetUserActiveAsync(username, active, cancellationToken);
        if (result.IsSuccess)
        {
            SuccessMessage = active ? "Utilisateur réactivé." : "Utilisateur désactivé.";
        }
        else
        {
            ErrorMessage = result.Error;
        }

        await LoadUsersAsync(cancellationToken);
        return Page();
    }

    private async Task LoadUsersAsync(CancellationToken cancellationToken)
    {
        Users = await _userManagementService.ListUsersAsync(cancellationToken);
        CostCenters = await _costCenterDirectory.ListAsync(cancellationToken);
    }

    public sealed class CreateUserInput
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int AccessLevel { get; set; } = 1;
        public string CostCenter { get; set; } = "CNSS";
    }
}
