using CNSS_TRESO.Domain.Enums;

namespace CNSS_TRESO.Application.DTOs;

public sealed record UpdateUserRequest(
    string Username,
    string Matricule,
    AccessLevel AccessLevel,
    string CostCenter
);