namespace CNSS_TRESO.Domain.Treasury;

public sealed class Journal
{
    public string Number { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public string? Type { get; private set; }
    public string? Account { get; private set; }
    public string? CostCenter { get; private set; }
    public string? Currency { get; private set; }
    public int? NextPiece { get; private set; }
    public string? MovementType { get; private set; }

    private Journal() { }

    public static Journal Create(string number, string? label, string? type, string? account, string costCenter, string? currency, int? nextPiece, string? movementType)
    {
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Le numero du journal est obligatoire.", nameof(number));
        if (string.IsNullOrWhiteSpace(costCenter)) throw new ArgumentException("Le centre de frais est obligatoire.", nameof(costCenter));
        if (number.Trim().Length > 10) throw new ArgumentException("Le numero du journal ne peut pas dépasser 10 caractères.", nameof(number));
        if (!string.IsNullOrWhiteSpace(label) && label.Trim().Length > 100) throw new ArgumentException("Le libellé ne peut pas dépasser 100 caractères.", nameof(label));
        if (!string.IsNullOrWhiteSpace(account) && account.Trim().Length > 14) throw new ArgumentException("Le compte ne peut pas dépasser 14 caractères.", nameof(account));
        if (costCenter.Trim().Length > 4) throw new ArgumentException("Le centre de frais ne peut pas dépasser 4 caractères.", nameof(costCenter));
        if (!string.IsNullOrWhiteSpace(currency) && currency.Trim().Length > 10) throw new ArgumentException("La monnaie ne peut pas dépasser 10 caractères.", nameof(currency));
        if (!string.IsNullOrWhiteSpace(movementType) && movementType.Trim().Length > 1) throw new ArgumentException("Le type de mouvement doit contenir un seul caractère.", nameof(movementType));

        return new Journal
        {
            Number = number.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim(),
            Account = string.IsNullOrWhiteSpace(account) ? null : account.Trim(),
            CostCenter = costCenter.Trim(),
            Currency = string.IsNullOrWhiteSpace(currency) ? null : currency.Trim(),
            NextPiece = nextPiece,
            MovementType = string.IsNullOrWhiteSpace(movementType) ? null : movementType.Trim()
        };
    }

    public bool IsCash => string.Equals(Type, "C", StringComparison.OrdinalIgnoreCase);
    public bool IsBank => string.Equals(Type, "B", StringComparison.OrdinalIgnoreCase);
}

public sealed class Signatory
{
    public string Journal { get; private set; } = string.Empty;
    public string? LeftName { get; private set; }
    public string? LeftTitle { get; private set; }
    public string? CenterName { get; private set; }
    public string? CenterTitle { get; private set; }
    public string? RightName { get; private set; }
    public string? RightTitle { get; private set; }
    public string? OfficerName { get; private set; }
    public string? OfficerTitle { get; private set; }
    public string? AccountantName { get; private set; }
    public string? AccountantTitle { get; private set; }
    public string? CostCenter { get; private set; }

    private Signatory() { }
}
