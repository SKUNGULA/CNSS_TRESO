using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CNSS_TRESO.Pages.Treasury.ExchangeRates;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IExchangeRateService _exchangeRateService;

    public IndexModel(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    public IReadOnlyList<ExchangeRate> Rates { get; private set; } = [];
    public IReadOnlyList<Currency> Currencies { get; private set; } = [];
    public List<SelectListItem> CurrencyOptions { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? FilterCurrency { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var rate = new ExchangeRate(
                DateOnly.FromDateTime(Input.RateDate),
                Input.CurrencyCode,
                Input.Amount,
                Input.Percent);
            var result = await _exchangeRateService.CreateExchangeRateAsync(rate, cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error;
                await LoadAsync(cancellationToken);
                return Page();
            }

            SuccessMessage = "Taux enregistré.";
            Input = new InputModel { RateDate = DateTime.Today, CurrencyCode = Input.CurrencyCode };
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int number, DateTime rateDate, string currencyCode, decimal amount, float percent, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.UpdateExchangeRateAsync(number, DateOnly.FromDateTime(rateDate), currencyCode, amount, percent, cancellationToken);
        if (result.IsSuccess) SuccessMessage = "Taux modifié."; else ErrorMessage = result.Error;
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int number, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.DeleteExchangeRateAsync(number, cancellationToken);
        if (result.IsSuccess) SuccessMessage = "Taux supprimé."; else ErrorMessage = result.Error;
        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Currencies = await _exchangeRateService.ListCurrenciesAsync(cancellationToken);
        CurrencyOptions = Currencies
            .Select(c => new SelectListItem(
                string.IsNullOrWhiteSpace(c.Label) ? c.Code : $"{c.Code} - {c.Label}",
                c.Code))
            .ToList();

        Rates = await _exchangeRateService.ListRecentAsync(200, FilterCurrency, cancellationToken);
    }

    public sealed class InputModel
    {
        public DateTime RateDate { get; set; } = DateTime.Today;
        public string CurrencyCode { get; set; } = "USD";
        public decimal Amount { get; set; }
        public float Percent { get; set; }
    }
}
