using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Treasury.Signatories;

[Authorize(Policy = Policies.AdministratorOnly)]
public class IndexModel : PageModel
{
    private readonly IJournalRepository _journals;
    private readonly ISignatoryRepository _signatories;

    public IndexModel(IJournalRepository journals, ISignatoryRepository signatories)
    {
        _journals = journals;
        _signatories = signatories;
    }

    public IReadOnlyList<Journal> Journals { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public string? JournalCode { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Journals = await _journals.ListAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(JournalCode) || !Journals.Any(journal => journal.Number == JournalCode))
        {
            ErrorMessage = "Veuillez sélectionner un journal existant.";
            return Page();
        }

        await _signatories.UpdateAsync(JournalCode, Input.LeftName, Input.LeftTitle, Input.OfficerName, Input.OfficerTitle, Input.RightName, Input.RightTitle, Input.CenterName, Input.CenterTitle, cancellationToken);
        SuccessMessage = "Signataires enregistrés.";
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Journals = await _journals.ListAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(JournalCode)) return;
        var signatory = await _signatories.GetByJournalAsync(JournalCode, cancellationToken);
        if (signatory is null) return;
        Input = new InputModel
        {
            LeftName = signatory.LeftName, LeftTitle = signatory.LeftTitle,
            OfficerName = signatory.OfficerName, OfficerTitle = signatory.OfficerTitle,
            RightName = signatory.RightName, RightTitle = signatory.RightTitle,
            CenterName = signatory.CenterName, CenterTitle = signatory.CenterTitle
        };
    }

    public sealed class InputModel
    {
        public string? LeftName { get; set; }
        public string? LeftTitle { get; set; }
        public string? OfficerName { get; set; }
        public string? OfficerTitle { get; set; }
        public string? RightName { get; set; }
        public string? RightTitle { get; set; }
        public string? CenterName { get; set; }
        public string? CenterTitle { get; set; }
    }
}