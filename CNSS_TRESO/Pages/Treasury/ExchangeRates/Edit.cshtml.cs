using CNSS_TRESO.Application.Treasury;
using CNSS_TRESO.Domain.Treasury;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages.Treasury.ExchangeRates;

[Authorize]
public class EditModel : PageModel
{
    private readonly IExchangeRateRepository _rates;
    private readonly IExchangeRateService _service;
    private readonly ICurrencyRepository _currencies;

    public EditModel(IExchangeRateRepository rates, IExchangeRateService service, ICurrencyRepository currencies)
    {
        _rates = rates;
        _service = service;
        _currencies = currencies;
    }

    [BindProperty(SupportsGet = true)]
    public int Number { get; set; }
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public IReadOnlyList<Currency> Currencies { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var rate = await _rates.GetByNumberAsync(Number, cancellationToken);
        if (rate is null) return NotFound();
        Currencies = await _currencies.GetAllAsync(cancellationToken);
        Input = new InputModel { RateDate = rate.RateDate.ToDateTime(TimeOnly.MinValue), CurrencyCode = rate.CurrencyCode, Amount = rate.Amount ?? 0, Percent = rate.Percent ?? 0 };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _service.UpdateExchangeRateAsync(Number, DateOnly.FromDateTime(Input.RateDate), Input.CurrencyCode, Input.Amount, Input.Percent, cancellationToken);
        if (result.IsSuccess) return RedirectToPage("./Index");
        ErrorMessage = result.Error;
        Currencies = await _currencies.GetAllAsync(cancellationToken);
        return Page();
    }

    public sealed class InputModel
    {
        public DateTime RateDate { get; set; } = DateTime.Today;
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public float Percent { get; set; }
    }
}