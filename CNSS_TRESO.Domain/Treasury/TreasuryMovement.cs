namespace CNSS_TRESO.Domain.Treasury;

public sealed class TreasuryMovement
{
    public int Number { get; private set; }
    public string Journal { get; private set; } = string.Empty;
    public int? PieceNumber { get; private set; }
    public string DocumentCode { get; private set; } = string.Empty;
    public int DocumentNumber { get; private set; }
    public DateOnly? MovementDate { get; private set; }
    public string? Description { get; private set; }
    public decimal? Amount { get; private set; }
    public decimal? Rate { get; private set; }
    public string? MovementType { get; private set; }
    public string? Direction { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? Account { get; private set; }
    public string? AccountType { get; private set; }
    public string? CostCenter { get; private set; }
    public string? Payer { get; private set; }
    public string? Beneficiary { get; private set; }
    public string? CheckNumber { get; private set; }
    public string? Bank { get; private set; }
    public string? Drawer { get; private set; }
    public string? CheckDate { get; private set; }
    public bool? Validated { get; private set; }
    public bool? Accounted { get; private set; }
    public string? UserName { get; private set; }
    public string? Society { get; private set; }
    public short? Period { get; private set; }
    public string? EntryDate { get; private set; }

    private TreasuryMovement() { }

    public TreasuryMovement(
        string journal,
        string documentCode,
        int documentNumber,
        DateOnly movementDate,
        decimal amount,
        decimal? rate,
        string currencyCode,
        string? description,
        string? direction,
        string? movementType,
        string? account,
        string? accountType,
        string? costCenter,
        string? payer,
        string? beneficiary,
        string? userName,
        string? society,
        short? period,
        int? pieceNumber = null,
        string? checkNumber = null,
        string? bank = null)
    {
        if (string.IsNullOrWhiteSpace(journal)) throw new ArgumentException("Journal requis.", nameof(journal));
        if (string.IsNullOrWhiteSpace(documentCode)) throw new ArgumentException("Document requis.", nameof(documentCode));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currencyCode)) throw new ArgumentException("Monnaie requise.", nameof(currencyCode));

        Journal = journal.Trim();
        DocumentCode = documentCode.Trim();
        DocumentNumber = documentNumber;
        MovementDate = movementDate;
        Amount = amount;
        Rate = rate;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        Description = description?.Trim();
        Direction = direction?.Trim();
        MovementType = movementType?.Trim();
        Account = account?.Trim();
        AccountType = accountType?.Trim();
        CostCenter = costCenter?.Trim();
        Payer = payer?.Trim();
        Beneficiary = beneficiary?.Trim();
        UserName = userName?.Trim();
        Society = society?.Trim();
        Period = period;
        PieceNumber = pieceNumber;
        CheckNumber = checkNumber?.Trim();
        Bank = bank?.Trim();
        Validated = false;
        Accounted = false;
        EntryDate = DateTime.Now.ToString("dd/MM/yyyy");
    }

    public void MarkValidated() => Validated = true;
    public void MarkUnvalidated() => Validated = false;

    public bool IsDraft => Validated != true;

    public void UpdateDraft(
        string documentCode,
        int documentNumber,
        DateOnly movementDate,
        decimal amount,
        decimal? rate,
        string currencyCode,
        string? description,
        string? direction,
        string? movementType,
        string? account,
        string? accountType,
        string? payer,
        string? beneficiary,
        int? pieceNumber,
        string? checkNumber,
        string? bank)
    {
        if (!IsDraft) throw new InvalidOperationException("Seul un mouvement brouillon peut etre modifie.");
        if (string.IsNullOrWhiteSpace(documentCode)) throw new ArgumentException("Document requis.", nameof(documentCode));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currencyCode)) throw new ArgumentException("Monnaie requise.", nameof(currencyCode));

        DocumentCode = documentCode.Trim();
        DocumentNumber = documentNumber;
        MovementDate = movementDate;
        Amount = amount;
        Rate = rate;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        Description = description?.Trim();
        Direction = direction?.Trim();
        MovementType = movementType?.Trim();
        Account = account?.Trim();
        AccountType = accountType?.Trim();
        Payer = payer?.Trim();
        Beneficiary = beneficiary?.Trim();
        PieceNumber = pieceNumber;
        CheckNumber = checkNumber?.Trim();
        Bank = bank?.Trim();
        Period = (short?)movementDate.Month;
    }
}
