using CNSS_TRESO.Application.DTOs;

namespace CNSS_TRESO.Application.Contracts;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(AuthenticationRequest request, CancellationToken cancellationToken = default);
}
