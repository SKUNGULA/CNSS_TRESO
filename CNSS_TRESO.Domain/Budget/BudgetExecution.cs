namespace CNSS_TRESO.Domain.Budget;

public sealed class BudgetExecution
{
    public int ExecutionBudgetId { get; private set; }
    public string NumeroTitre { get; private set; } = string.Empty;
    public int CentreGestionId { get; private set; }
    public string? Monnaie { get; private set; }
    public decimal? Montant { get; private set; }
    public decimal? Taux { get; private set; }
    public decimal? MontantPaye { get; private set; }
    public decimal? MontantPayeTP { get; private set; }
    public DateOnly? DatePaiement { get; private set; }
    public int? EtatTreso { get; private set; }

    private BudgetExecution() { }
}

public sealed class ParamCentreGestion
{
    public int CentreGestionId { get; private set; }
    public string CodeCentreGestion { get; private set; } = string.Empty;
    public string? CentreGestion { get; private set; }

    private ParamCentreGestion() { }
}
