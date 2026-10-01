using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Treasury.Journals;

[Authorize(Policy = Policies.AdministratorOnly)]
public class IndexModel : PageModel
{
    private readonly IJournalRepository _journals;
    private readonly IDocumentRepository _documents;
    private readonly ICostCenterDirectory _costCenters;
    private readonly ICurrencyRepository _currencies;

    public IndexModel(IJournalRepository journals, IDocumentRepository documents, ICostCenterDirectory costCenters, ICurrencyRepository currencies)
    {
        _journals = journals;
        _documents = documents;
        _costCenters = costCenters;
        _currencies = currencies;
    }

    public IReadOnlyList<Journal> Journals { get; private set; } = [];
    public IReadOnlyList<Document> Documents { get; private set; } = [];
    public IReadOnlyList<CostCenterOption> CostCenters { get; private set; } = [];
    public IReadOnlyList<Currency> Currencies { get; private set; } = [];

    [BindProperty]
    public JournalInput NewJournal { get; set; } = new();

    [BindProperty]
    public DocumentInput NewDocument { get; set; } = new();

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);

    public async Task<IActionResult> OnPostCreateJournalAsync(CancellationToken cancellationToken)
    {
        if (NewJournal.Number.Trim().Length > 10)
            ModelState.AddModelError("NewJournal.Number", "Le numéro du journal ne peut pas dépasser 10 caractères.");
        if (!string.IsNullOrWhiteSpace(NewJournal.Account) && NewJournal.Account.Trim().Length > 14)
            ModelState.AddModelError("NewJournal.Account", "Le compte ne peut pas dépasser 14 caractères.");

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs signalés.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        if (await _journals.GetByNumberAsync(NewJournal.Number, cancellationToken) is not null)
        {
            ErrorMessage = "Ce numéro de journal existe déjà.";
        }
        else
        {
            try
            {
                await _journals.AddAsync(Journal.Create(NewJournal.Number, NewJournal.Label, NewJournal.Type, NewJournal.Account, NewJournal.CostCenter, NewJournal.Currency, NewJournal.NextPiece, NewJournal.MovementType), cancellationToken);
                SuccessMessage = "Journal créé avec succès.";
                NewJournal = new JournalInput();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateDocumentAsync(CancellationToken cancellationToken)
    {
        if (NewDocument.Code.Trim().Length > 8)
            ModelState.AddModelError("NewDocument.Code", "Le code du document ne peut pas dépasser 8 caractères.");
        if (NewDocument.Journal.Trim().Length > 10)
            ModelState.AddModelError("NewDocument.Journal", "Le numéro du journal ne peut pas dépasser 10 caractères.");
        if (!string.IsNullOrWhiteSpace(NewDocument.MovementType) && NewDocument.MovementType.Trim().Length > 3)
            ModelState.AddModelError("NewDocument.MovementType", "Le type de mouvement ne peut pas dépasser 3 caractères.");

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs signalés.";
            await LoadAsync(cancellationToken);
            return Page();
        }

        if (await _journals.GetByNumberAsync(NewDocument.Journal, cancellationToken) is null)
        {
            ErrorMessage = "Veuillez choisir un journal existant.";
        }
        else
        {
            try
            {
                await _documents.AddAsync(Document.Create(NewDocument.Code, NewDocument.Label, NewDocument.Journal, NewDocument.Number, NewDocument.MovementType, NewDocument.Direction, NewDocument.JournalType, NewDocument.CurrencyCode), cancellationToken);
                SuccessMessage = "Document créé avec succès.";
                NewDocument = new DocumentInput();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Journals = await _journals.ListAsync(cancellationToken);
        Documents = await _documents.ListAsync(cancellationToken: cancellationToken);
        CostCenters = await _costCenters.ListAsync(cancellationToken);
        Currencies = await _currencies.GetAllAsync(cancellationToken);
    }

    public sealed class JournalInput
    {
        public string Number { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string Type { get; set; } = "C";
        public string? Account { get; set; }
        public string CostCenter { get; set; } = string.Empty;
        public string? Currency { get; set; }
        public int? NextPiece { get; set; }
        public string? MovementType { get; set; }
    }

    public sealed class DocumentInput
    {
        public string Code { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string Journal { get; set; } = string.Empty;
        public int? Number { get; set; }
        public string? MovementType { get; set; }
        public string Direction { get; set; } = "D";
        public string? JournalType { get; set; }
        public string? CurrencyCode { get; set; }
    }
}