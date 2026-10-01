namespace CNSS_TRESO.Domain.Treasury;

public sealed class ExchangeRate
{
    public int Number { get; private set; }
    public DateOnly RateDate { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public decimal? Amount { get; private set; }
    public float? Percent { get; private set; }

    private ExchangeRate() { }

    public ExchangeRate(DateOnly rateDate, string currencyCode, decimal amount, float percent = 0f)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) throw new ArgumentException("Currency code is required.", nameof(currencyCode));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        RateDate = rateDate;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        Amount = amount;
        Percent = percent;
    }

    public void Update(DateOnly rateDate, string currencyCode, decimal amount, float percent)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) throw new ArgumentException("Currency code is required.", nameof(currencyCode));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        RateDate = rateDate;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        Amount = amount;
        Percent = percent;
    }
}
