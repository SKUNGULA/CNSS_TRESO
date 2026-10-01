using CNSS_TRESO.Domain.Enums;

namespace CNSS_TRESO.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Username,
    string FullName,
    AccessLevel AccessLevel,
    string CostCenter,
    bool IsActive,
    string Matricule
);
