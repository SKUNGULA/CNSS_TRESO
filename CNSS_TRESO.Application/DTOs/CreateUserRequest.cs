using CNSS_TRESO.Domain.Enums;

namespace CNSS_TRESO.Application.DTOs;

public sealed record CreateUserRequest(
    string Username,
    string FullName,
    string PlainPassword,
    AccessLevel AccessLevel,
    string CostCenter
);
