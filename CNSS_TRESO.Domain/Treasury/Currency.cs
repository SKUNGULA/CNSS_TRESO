namespace CNSS_TRESO.Domain.Treasury;

public sealed class Currency
{
    public string Code { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public string? Symbol { get; private set; }

    private Currency() { }

    public Currency(string code, string? label, string? symbol)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        Code = code.Trim();
        Label = label;
        Symbol = symbol;
    }
}
