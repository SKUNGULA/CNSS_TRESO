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
public class CreateModel : PageModel
{
    private readonly ITreasuryMovementService _movementService;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IComptaRepository _comptaRepository;
    private readonly IUserJournalRepository _journalRepository;
    private readonly IExchangeRateRepository _rateRepository;

    public CreateModel(ITreasuryMovementService movementService, ICurrencyRepository currencyRepository, IComptaRepository comptaRepository, IUserJournalRepository journalRepository, IExchangeRateRepository rateRepository)
    {
        _movementService = movementService;
        _currencyRepository = currencyRepository;
        _comptaRepository = comptaRepository;
        _journalRepository = journalRepository;
        _rateRepository = rateRepository;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Nature { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public IReadOnlyList<Document> Documents { get; private set; } = [];
    public IReadOnlyList<Currency> Currencies { get; private set; } = [];
    public IReadOnlyList<TiersOption> TiersList { get; private set; } = [];
    public IReadOnlyList<UserJournal> Journals { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        Input.MovementDate = DateTime.Today;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        var caller = BuildCaller();

        var request = new CreateMovementRequest(
            Journal: Input.Journal,
            DocumentCode: Input.DocumentCode,
            DocumentNumber: Input.DocumentNumber,
            MovementDate: DateOnly.FromDateTime(Input.MovementDate),
            Amount: Input.Amount,
            Rate: Input.Rate,
            CurrencyCode: Input.CurrencyCode,
            Description: Input.Description,
            Direction: Input.Direction,
            MovementType: Input.MovementType,
            Account: Input.Account,
            AccountType: Input.AccountType,
            Payer: Input.Payer,
            Beneficiary: Input.Beneficiary,
            PieceNumber: Input.PieceNumber,
            CheckNumber: Input.CheckNumber,
            Bank: Input.Bank);

        var result = await _movementService.CreateMovementAsync(request, caller, cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }

        SuccessMessage = $"Mouvement enregistré (N° {result.Number}).";
        if (!string.IsNullOrWhiteSpace(result.Error)) SuccessMessage += " " + result.Error;
        Input = new InputModel { MovementDate = DateTime.Today, CurrencyCode = Input.CurrencyCode, Journal = Input.Journal };
        return Page();
    }

    public async Task<IActionResult> OnGetAccountsAsync(string tiers, CancellationToken cancellationToken)
    {
        var cf = User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExt.CostCenter)?.Value;
        var list = await _comptaRepository.ListAccountsAsync(tiers ?? string.Empty, cf, cancellationToken);
        return new JsonResult(list.Select(a => new { code = a.Code, label = a.Label, general = a.GeneralAccount }));
    }

    public async Task<IActionResult> OnGetRateAsync(string currency, DateTime date, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return new JsonResult(new { rate = (decimal?)null });
        var d = DateOnly.FromDateTime(date == default ? DateTime.Today : date);
        var rate = await _rateRepository.GetAmountAsync(currency, d, cancellationToken);
        return new JsonResult(new { rate });
    }

    public async Task<IActionResult> OnGetJournalAsync(string number, CancellationToken cancellationToken)
    {
        if (!await IsJournalAuthorizedAsync(number, cancellationToken)) return Forbid();
        var j = await _journalRepository.GetAsync(number ?? string.Empty, cancellationToken);
        if (j is null) return new JsonResult(null);
        return new JsonResult(new { currency = j.Currency, movementType = j.MovementType, nextPiece = j.NextPiece });
    }

    public async Task<IActionResult> OnGetDocumentsAsync(string journal, CancellationToken cancellationToken)
    {
        if (!await IsJournalAuthorizedAsync(journal, cancellationToken)) return Forbid();
        var list = await _movementService.ListDocumentsByJournalAsync(journal ?? string.Empty, cancellationToken);
        return new JsonResult(list.Select(d => new { code = d.Code, label = d.Label, nextNumber = d.Number, direction = d.Direction, movementType = d.MovementType }));
    }

    public async Task<IActionResult> OnGetDailyMovementsAsync(string journal, DateTime date, CancellationToken cancellationToken)
    {
        if (!await IsJournalAuthorizedAsync(journal, cancellationToken)) return Forbid();

        var caller = BuildCaller();
        var movementDate = DateOnly.FromDateTime(date == default ? DateTime.Today : date);
        var movements = await _movementService.SearchAsync(new MovementFilter(
            FromDate: movementDate,
            ToDate: movementDate,
            Journal: journal.Trim(),
            CostCenter: caller.CostCenter,
            Take: 300), cancellationToken);

        return new JsonResult(movements.Select(m => new
        {
            number = m.Number,
            pieceNumber = m.PieceNumber,
            documentCode = m.DocumentCode,
            documentNumber = m.DocumentNumber,
            description = m.Description,
            direction = m.Direction,
            amount = m.Amount,
            currencyCode = m.CurrencyCode,
            validated = m.Validated == true
        }));
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var caller = BuildCaller();
        Documents = await _movementService.ListDocumentsAsync(cancellationToken: cancellationToken);
        Currencies = await _currencyRepository.GetAllAsync(cancellationToken);
        TiersList = await _comptaRepository.ListTiersAsync(cancellationToken);
        Journals = await _journalRepository.ListAsync(caller.Username, caller.CostCenter, cancellationToken);
    }

    private async Task<bool> IsJournalAuthorizedAsync(string? journal, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(journal)) return false;
        var caller = BuildCaller();
        var journals = await _journalRepository.ListAsync(caller.Username, caller.CostCenter, cancellationToken);
        return journals.Any(item => string.Equals(item.Number, journal.Trim(), StringComparison.OrdinalIgnoreCase));
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

    public sealed class InputModel
    {
        public string Journal { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public int DocumentNumber { get; set; }
        public int? PieceNumber { get; set; }
        public DateTime MovementDate { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        public decimal? Rate { get; set; }
        public string CurrencyCode { get; set; } = "CDF";
        public string? Description { get; set; }
        public string? Direction { get; set; } = "D";
        public string? MovementType { get; set; } = "Esp";
        public string? AccountType { get; set; }
        public string? Account { get; set; }
        public string? Payer { get; set; }
        public string? Beneficiary { get; set; }
        public string? CheckNumber { get; set; }
        public string? Bank { get; set; }
    }
}
