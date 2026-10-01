using CNSS_TRESO.Application.Contracts;
using CNSS_TRESO.Application.DTOs;
using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Users;

[Authorize(Policy = Policies.AdministratorOnly)]
public class JournalsModel : PageModel
{
    private readonly IUserManagementService _users;
    private readonly IJournalRepository _journals;
    private readonly IUserJournalRepository _userJournals;

    public JournalsModel(IUserManagementService users, IJournalRepository journals, IUserJournalRepository userJournals)
    {
        _users = users;
        _journals = journals;
        _userJournals = userJournals;
    }

    [BindProperty(SupportsGet = true)]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public List<string> SelectedJournals { get; set; } = [];

    public UserDto? TargetUser { get; private set; }
    public IReadOnlyList<Journal> AvailableJournals { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!await LoadAsync(cancellationToken, loadAssignments: true)) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!await LoadAsync(cancellationToken, loadAssignments: false)) return NotFound();

        var allowedNumbers = AvailableJournals.Select(journal => journal.Number).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (SelectedJournals.Any(number => !allowedNumbers.Contains(number)))
        {
            ErrorMessage = "Un journal sélectionné ne correspond pas au centre de frais de cet utilisateur.";
            return Page();
        }

        try
        {
            await _userJournals.ReplaceAssignmentsAsync(Username, SelectedJournals, cancellationToken);
            SuccessMessage = "Journaux attribués avec succès.";
        }
        catch (Exception)
        {
            ErrorMessage = "Les journaux n'ont pas pu être attribués.";
        }

        await LoadAsync(cancellationToken, loadAssignments: true);
        return Page();
    }

    private async Task<bool> LoadAsync(CancellationToken cancellationToken, bool loadAssignments)
    {
        TargetUser = await _users.GetUserAsync(Username, cancellationToken);
        if (TargetUser is null || !string.Equals(TargetUser.Username, Username, StringComparison.OrdinalIgnoreCase)) return false;

        var allJournals = await _journals.ListAsync(cancellationToken);
        AvailableJournals = allJournals
            .Where(journal => string.Equals(journal.CostCenter, TargetUser.CostCenter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(journal => journal.Number)
            .ToList();

        if (loadAssignments)
        {
            var assigned = await _userJournals.ListAsync(TargetUser.Username, TargetUser.CostCenter, cancellationToken);
            SelectedJournals = assigned.Select(journal => journal.Number).ToList();
        }
        return true;
    }
}