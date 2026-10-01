namespace CNSS_TRESO.Domain.Treasury;

public sealed class Document
{
    public string Code { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public string? Journal { get; private set; }
    public int? Number { get; private set; }
    public string? MovementType { get; private set; }
    public string? Direction { get; private set; }
    public string? JournalType { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? BedRequired { get; private set; } = "0";
    public int Num { get; private set; }

    private Document() { }

    public static Document Create(string code, string? label, string journal, int? number, string? movementType, string? direction, string? journalType, string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Le code du document est obligatoire.", nameof(code));
        if (string.IsNullOrWhiteSpace(journal)) throw new ArgumentException("Le journal est obligatoire.", nameof(journal));
        if (code.Trim().Length > 8) throw new ArgumentException("Le code du document ne peut pas dépasser 8 caractères.", nameof(code));
        if (journal.Trim().Length > 10) throw new ArgumentException("Le numéro du journal ne peut pas dépasser 10 caractères.", nameof(journal));
        if (!string.IsNullOrWhiteSpace(movementType) && movementType.Trim().Length > 3) throw new ArgumentException("Le type de mouvement ne peut pas dépasser 3 caractères.", nameof(movementType));
        if (!string.IsNullOrWhiteSpace(direction) && direction.Trim().Length > 1) throw new ArgumentException("Le sens du mouvement doit contenir un seul caractère.", nameof(direction));
        if (!string.IsNullOrWhiteSpace(journalType) && journalType.Trim().Length > 1) throw new ArgumentException("Le type de journal doit contenir un seul caractère.", nameof(journalType));
        if (!string.IsNullOrWhiteSpace(currencyCode) && currencyCode.Trim().Length > 6) throw new ArgumentException("La monnaie ne peut pas dépasser 6 caractères.", nameof(currencyCode));

        return new Document
        {
            Code = code.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            Journal = journal.Trim(),
            Number = number,
            MovementType = string.IsNullOrWhiteSpace(movementType) ? null : movementType.Trim(),
            Direction = string.IsNullOrWhiteSpace(direction) ? null : direction.Trim(),
            JournalType = string.IsNullOrWhiteSpace(journalType) ? null : journalType.Trim(),
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim(),
            BedRequired = "0"
        };
    }
}
