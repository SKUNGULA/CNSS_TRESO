using CNSS_TRESO.Application.Auditing;
using CNSS_TRESO.Application.Budget;
using CNSS_TRESO.Application.Security;
using CNSS_TRESO.Domain.Treasury;

namespace CNSS_TRESO.Application.Treasury;

public sealed class TreasuryMovementService : ITreasuryMovementService
{
    private readonly ITreasuryMovementRepository _repository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IBudgetIntegrationService _budget;
    private readonly ISuiviTpRepository _suiviTp;
    private readonly IAuditTrailService _audit;

    public TreasuryMovementService(ITreasuryMovementRepository repository, IDocumentRepository documentRepository, IBudgetIntegrationService budget, ISuiviTpRepository suiviTp, IAuditTrailService audit)
    {
        _repository = repository;
        _documentRepository = documentRepository;
        _budget = budget;
        _suiviTp = suiviTp;
        _audit = audit;
    }

    public async Task<(bool IsSuccess, string? Error, int? Number)> CreateMovementAsync(CreateMovementRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (request is null) return (false, "Mouvement obligatoire.", null);
        if (string.IsNullOrWhiteSpace(request.Journal)) return (false, "Journal obligatoire.", null);
        if (string.IsNullOrWhiteSpace(request.DocumentCode)) return (false, "Document obligatoire.", null);
        if (request.Amount <= 0) return (false, "Montant doit être positif.", null);
        if (string.IsNullOrWhiteSpace(request.CurrencyCode)) return (false, "Monnaie obligatoire.", null);
        if (request.Rate is not > 0) return (false, "Taux obligatoire et strictement positif.", null);
        if (caller.Exercise > 0 && request.MovementDate.Year != caller.Exercise)
            return (false, $"La date doit être dans l'exercice {caller.Exercise}.", null);
        if (string.IsNullOrWhiteSpace(request.Direction) || (request.Direction != "D" && request.Direction != "R"))
            return (false, "Sens du mouvement obligatoire (D ou R).", null);
        if (string.IsNullOrWhiteSpace(request.AccountType))
            return (false, "Tiers obligatoire.", null);
        if (string.IsNullOrWhiteSpace(request.Account))
            return (false, "Compte d'imputation obligatoire.", null);
        if (string.IsNullOrWhiteSpace(caller.CostCenter))
            return (false, "Centre de frais utilisateur introuvable.", null);

        // Règles legacy : pour un chèque, n° chèque + banque obligatoires.
        if (string.Equals(request.MovementType, "Chq", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.CheckNumber)) return (false, "N° chèque obligatoire pour un mouvement chèque.", null);
            if (string.IsNullOrWhiteSpace(request.Bank)) return (false, "Banque obligatoire pour un mouvement chèque.", null);
        }

        // Stamping legacy : User, cf, Société=exer, Période=mois
        // Legacy : Société stocké sur 2 caractères (année à 2 chiffres, ex. "25")
        var society = caller.Exercise > 0 ? (caller.Exercise % 100).ToString("D2") : null;
        var costCenter = string.IsNullOrWhiteSpace(caller.CostCenter) ? null : caller.CostCenter;

        try
        {
            var movement = new TreasuryMovement(
                request.Journal, request.DocumentCode, request.DocumentNumber,
                request.MovementDate, request.Amount, request.Rate, request.CurrencyCode,
                request.Description, request.Direction, request.MovementType,
                request.Account, request.AccountType, costCenter, request.Payer, request.Beneficiary,
                userName: caller.Username, society: society, period: (short?)(request.MovementDate.Month),
                request.PieceNumber, request.CheckNumber, request.Bank);

            var number = await _repository.AddAsync(movement, cancellationToken);
            await _audit.RecordAsync(caller.Username, "Movement.Create", "Movement", number.ToString(), $"Journal={request.Journal} Doc={request.DocumentCode}/{request.DocumentNumber} Montant={request.Amount} {request.CurrencyCode} Sens={request.Direction} CF={costCenter}", cancellationToken);

            // Legacy save_mouv : marquer le titre validé final dans SUIVI_TP..coco
            if (movement.PieceNumber is > 0 && !string.IsNullOrWhiteSpace(costCenter))
            {
                try
                {
                    await _suiviTp.MarkFinalValidatedAsync(
                        movement.PieceNumber!.Value.ToString(),
                        costCenter!,
                        movement.MovementDate ?? request.MovementDate,
                        caller.Username,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    return (true, $"Mouvement {number} créé. Avertissement SUIVI_TP : {ex.Message}", number);
                }
            }

            return (true, null, number);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            var inner = ex.InnerException;
            while (inner is not null)
            {
                msg += " >>> " + inner.Message;
                inner = inner.InnerException;
            }
            return (false, msg, null);
        }
    }

    public Task<IReadOnlyList<TreasuryMovement>> SearchAsync(MovementFilter filter, CancellationToken cancellationToken = default)
        => _repository.SearchAsync(filter, cancellationToken);

    public Task<TreasuryMovement?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
        => _repository.GetByNumberAsync(number, cancellationToken);

    public async Task<(bool IsSuccess, string? Error)> UpdateDraftAsync(int number, CreateMovementRequest request, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (request is null) return (false, "Donnees obligatoires.");
        var m = await _repository.GetByNumberAsync(number, cancellationToken);
        if (m is null) return (false, "Mouvement introuvable.");
        if (!m.IsDraft) return (false, "Seul un mouvement brouillon peut etre modifie.");

        if (!string.Equals(m.UserName, caller.Username, StringComparison.OrdinalIgnoreCase) && !caller.IsAdministrator)
            return (false, "Vous ne pouvez modifier que vos propres brouillons.");

        if (request.Amount <= 0) return (false, "Montant doit etre positif.");
        if (string.IsNullOrWhiteSpace(request.CurrencyCode)) return (false, "Monnaie obligatoire.");
        if (request.Rate is not > 0) return (false, "Taux obligatoire et strictement positif.");
        if (string.IsNullOrWhiteSpace(request.Direction) || (request.Direction != "D" && request.Direction != "R"))
            return (false, "Sens du mouvement obligatoire (D ou R).");
        if (caller.Exercise > 0 && request.MovementDate.Year != caller.Exercise)
            return (false, $"La date doit etre dans l'exercice {caller.Exercise}.");
        if (string.Equals(request.MovementType, "Chq", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.CheckNumber)) return (false, "N° cheque obligatoire pour un mouvement cheque.");
            if (string.IsNullOrWhiteSpace(request.Bank)) return (false, "Banque obligatoire pour un mouvement cheque.");
        }

        try
        {
            m.UpdateDraft(
                request.DocumentCode, request.DocumentNumber, request.MovementDate,
                request.Amount, request.Rate, request.CurrencyCode,
                request.Description, request.Direction, request.MovementType,
                request.Account, request.AccountType, request.Payer, request.Beneficiary,
                request.PieceNumber, request.CheckNumber, request.Bank);

            await _repository.UpdateDraftAsync(m, cancellationToken);
            await _audit.RecordAsync(caller.Username, "Movement.Update", "Movement", number.ToString(),
                $"Journal={m.Journal} Doc={m.DocumentCode}/{m.DocumentNumber} Montant={m.Amount} {m.CurrencyCode} Sens={m.Direction}",
                cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool IsSuccess, string? Error)> DeleteDraftAsync(int number, string reason, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 4)
            return (false, "Motif de suppression obligatoire (4 caracteres minimum).");

        var m = await _repository.GetByNumberAsync(number, cancellationToken);
        if (m is null) return (false, "Mouvement introuvable.");
        if (!m.IsDraft) return (false, "Seul un mouvement brouillon peut etre supprime.");
        if (!string.Equals(m.UserName, caller.Username, StringComparison.OrdinalIgnoreCase) && !caller.IsAdministrator)
            return (false, "Vous ne pouvez supprimer que vos propres brouillons.");

        await _audit.RecordAsync(caller.Username, "Movement.Delete", "Movement", number.ToString(),
            $"Journal={m.Journal} Doc={m.DocumentCode}/{m.DocumentNumber} Montant={m.Amount} {m.CurrencyCode} Sens={m.Direction} Motif={reason.Trim()}",
            cancellationToken);
        await _repository.DeleteAsync(number, cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> ValidateAsync(int number, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (!caller.IsAdministrator) return (false, "Seul un administrateur peut valider.");
        var m = await _repository.GetByNumberAsync(number, cancellationToken);
        if (m is null) return (false, "Mouvement introuvable.");
        if (m.Validated == true) return (false, "Déjà validé.");
        await _repository.UpdateValidationAsync(number, true, cancellationToken);
        await _audit.RecordAsync(caller.Username, "Movement.Validate", "Movement", number.ToString(), $"Journal={m.Journal} Sens={m.Direction} Montant={m.Amount} {m.CurrencyCode}", cancellationToken);

        if (string.Equals(m.Direction, "D", StringComparison.OrdinalIgnoreCase) && m.PieceNumber is > 0)
        {
            var payDate = m.MovementDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var bud = await _budget.SettleAsync(m, payDate, cancellationToken);
            if (!bud.IsSuccess)
                return (true, $"Mouvement validé. Avertissement BUDGET: {bud.Message}");
        }
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> ValidateGroupAsync(string journal, DateOnly movementDate, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (!caller.IsAdministrator) return (false, "Seul un administrateur peut valider.");
        if (string.IsNullOrWhiteSpace(journal)) return (false, "Journal obligatoire.");

        var count = await _repository.UpdateValidationByJournalAndDateAsync(journal.Trim(), movementDate, true, cancellationToken);
        if (count == 0) return (false, "Aucun mouvement non validé pour ce journal et cette date.");
        await _audit.RecordAsync(caller.Username, "Movement.ValidateGroup", "MovementGroup", $"{journal}/{movementDate:yyyy-MM-dd}", $"Journal={journal} Date={movementDate:yyyy-MM-dd} Nombre={count}", cancellationToken);
        return (true, $"{count} mouvement(s) validé(s).");
    }

    public async Task<(bool IsSuccess, string? Error)> DevalidateAsync(int number, string reason, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (!caller.IsAdministrator) return (false, "Seul un administrateur peut dévalider.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 4) return (false, "Motif de dévalidation obligatoire (4 caractères minimum).");
        var m = await _repository.GetByNumberAsync(number, cancellationToken);
        if (m is null) return (false, "Mouvement introuvable.");
        if (m.Validated != true) return (false, "Seuls les mouvements validés peuvent être dévalidés.");
        await _repository.UpdateValidationAsync(number, false, cancellationToken);
        await _audit.RecordAsync(caller.Username, "Movement.Devalidate", "Movement", number.ToString(), $"Journal={m.Journal} Sens={m.Direction} Montant={m.Amount} {m.CurrencyCode} Motif={reason.Trim()}", cancellationToken);

        if (string.Equals(m.Direction, "D", StringComparison.OrdinalIgnoreCase) && m.PieceNumber is > 0)
        {
            var bud = await _budget.RevertAsync(m, cancellationToken);
            if (!bud.IsSuccess)
                return (true, $"Mouvement dévalidé. Avertissement BUDGET: {bud.Message}");
        }
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? Error)> DevalidateGroupAsync(string journal, DateOnly movementDate, string reason, CallerContext caller, CancellationToken cancellationToken = default)
    {
        if (!caller.IsAdministrator) return (false, "Seul un administrateur peut dévalider.");
        if (string.IsNullOrWhiteSpace(journal)) return (false, "Journal obligatoire.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 4) return (false, "Motif de dévalidation obligatoire (4 caractères minimum).");

        var count = await _repository.UpdateValidationByJournalAndDateAsync(journal.Trim(), movementDate, false, cancellationToken);
        if (count == 0) return (false, "Aucun mouvement validé pour ce journal et cette date.");
        await _audit.RecordAsync(caller.Username, "Movement.DevalidateGroup", "MovementGroup", $"{journal}/{movementDate:yyyy-MM-dd}", $"Journal={journal} Date={movementDate:yyyy-MM-dd} Nombre={count} Motif={reason.Trim()}", cancellationToken);
        return (true, $"{count} mouvement(s) dévalidé(s).");
    }

    public Task<IReadOnlyList<Document>> ListDocumentsAsync(string? journalType = null, CancellationToken cancellationToken = default)
        => _documentRepository.ListAsync(journalType, cancellationToken);

    public Task<IReadOnlyList<Document>> ListDocumentsByJournalAsync(string journalNumber, CancellationToken cancellationToken = default)
        => _documentRepository.ListByJournalAsync(journalNumber, cancellationToken);
}
