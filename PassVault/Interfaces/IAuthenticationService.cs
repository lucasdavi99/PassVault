using PassVault.Services.Security;

namespace PassVault.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AppAuthenticationResult> AuthenticateAsync(AppAuthenticationRequest request, CancellationToken cancellationToken = default);
    }
}
