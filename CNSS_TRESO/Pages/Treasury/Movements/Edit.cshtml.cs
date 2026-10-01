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
public class EditModel : PageModel
{
    private readonly ITreasuryMovementService _movementService;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IComptaRepository _comptaRepository;
    private readonly IUserJournalRepository _journalRepository;
    private readonly IExchangeRateRepository _rateRepository;

    public EditModel(
        ITreasuryMovementService movementService,
        ICurrencyRepository currencyRepository,
        IComptaRepository comptaRepository,
        IUserJournalRepository journalRepository,
        IExchangeRateRepository rateRepository)
    {
        _movementService = movementService;
        _currencyRepository = currencyRepository;
        _comptaRepository = comptaRepository;
        _journalRepository = journalRepository;
        _rateRepository = rateRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string Journal { get; private set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public IReadOnlyList<Document> Documents { get; private set; } = [];
    public IReadOnlyList<Currency> Currencies { get; private set; } = [];
    public IReadOnlyList<TiersOption> TiersList { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var m = await _movementService.GetByNumberAsync(Number, cancellationToken);
        if (m is null) return NotFound();
        if (!m.IsDraft) { ErrorMessage = "Seul un mouvement brouillon peut etre modifie."; await LoadAsync(cancellationToken); Journal = m.Journal; return Page(); }

        Journal = m.Journal;
        Input = new InputModel
        {
            DocumentCode = m.DocumentCode,
            DocumentNumber = m.DocumentNumber,
            PieceNumber = m.PieceNumber,
            MovementDate = m.MovementDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today,
            Amount = m.Amount ?? 0m,
            Rate = m.Rate,
            CurrencyCode = m.CurrencyCode ?? "CDF",
            Description = m.Description,
            Direction = m.Direction ?? "D",
            MovementType = m.MovementType ?? "Esp",
            AccountType = m.AccountType,
            Account = m.Account,
            Payer = m.Payer,
            Beneficiary = m.Beneficiary,
            CheckNumber = m.CheckNumber,
            Bank = m.Bank
        };
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var caller = BuildCaller();
        var m = await _movementService.GetByNumberAsync(Number, cancellationToken);
        if (m is null) return NotFound();
        Journal = m.Journal;

        var request = new CreateMovementRequest(
            Journal: m.Journal,
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

        var result = await _movementService.UpdateDraftAsync(Number, request, caller, cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            await LoadAsync(cancellationToken);
            return Page();
        }

        return RedirectToPage("Index", new { SuccessMessage = $"Mouvement {Number} mis a jour." });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Documents = await _movementService.ListDocumentsAsync(cancellationToken: cancellationToken);
        Currencies = await _currencyRepository.GetAllAsync(cancellationToken);
        TiersList = await _comptaRepository.ListTiersAsync(cancellationToken);
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
