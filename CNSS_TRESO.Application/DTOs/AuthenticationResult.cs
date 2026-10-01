using CNSS_TRESO.Domain.Enums;

namespace CNSS_TRESO.Application.DTOs;

public sealed record AuthenticationResult(
    bool IsSuccess,
    string? Error,
    Guid? UserId,
    string? Username,
    string? FullName,
    AccessLevel? AccessLevel,
    string? CostCenter
);
