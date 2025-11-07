#if WINDOWS
using Windows.Security.Credentials.UI;
#else
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
#endif
using System.Collections.Generic;
using PassVault.Interfaces;

namespace PassVault.Services.Security
{
    public class AuthenticationService : IAuthenticationService
    {
        public async Task<AppAuthenticationResult> AuthenticateAsync(AppAuthenticationRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

#if WINDOWS
            return await AuthenticateOnWindowsAsync(request);
#else
            return await AuthenticateWithFingerprintAsync(request, cancellationToken);
#endif
        }

#if WINDOWS
        private static async Task<AppAuthenticationResult> AuthenticateOnWindowsAsync(AppAuthenticationRequest request)
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                return availability switch
                {
                    UserConsentVerifierAvailability.DeviceNotPresent => AppAuthenticationResult.NotAvailable("Device not present."),
                    UserConsentVerifierAvailability.DisabledByPolicy => AppAuthenticationResult.NotAvailable("Disabled by policy."),
                    UserConsentVerifierAvailability.NotConfiguredForUser => AppAuthenticationResult.NotAvailable("User not configured."),
                    UserConsentVerifierAvailability.DeviceBusy => AppAuthenticationResult.Failed("Device busy."),
                    _ => AppAuthenticationResult.NotAvailable("Authentication unavailable.")
                };
            }

            string prompt = BuildPrompt(request);
            var result = await UserConsentVerifier.RequestVerificationAsync(prompt);

            return result switch
            {
                UserConsentVerificationResult.Verified => AppAuthenticationResult.Success(),
                UserConsentVerificationResult.Canceled => AppAuthenticationResult.Canceled(),
                UserConsentVerificationResult.DeviceNotPresent => AppAuthenticationResult.NotAvailable("Device not present."),
                UserConsentVerificationResult.DisabledByPolicy => AppAuthenticationResult.NotAvailable("Disabled by policy."),
                UserConsentVerificationResult.NotConfiguredForUser => AppAuthenticationResult.NotAvailable("User not configured."),
                UserConsentVerificationResult.DeviceBusy => AppAuthenticationResult.Failed("Device busy."),
                UserConsentVerificationResult.RetriesExhausted => AppAuthenticationResult.Failed("Retries exhausted."),
                _ => AppAuthenticationResult.Failed("Authentication failed.")
            };
        }

        private static string BuildPrompt(AppAuthenticationRequest request)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Title))
                parts.Add(request.Title.Trim());

            if (!string.IsNullOrWhiteSpace(request.Message) &&
                (parts.Count == 0 || !string.Equals(parts[0], request.Message.Trim(), StringComparison.Ordinal)))
            {
                parts.Add(request.Message.Trim());
            }

            return parts.Count == 0 ? "Authenticate to continue" : string.Join(Environment.NewLine, parts);
        }
#else
        private static async Task<AppAuthenticationResult> AuthenticateWithFingerprintAsync(AppAuthenticationRequest request, CancellationToken cancellationToken)
        {
            var title = string.IsNullOrWhiteSpace(request.Title) ? "PassVault" : request.Title;
            var message = string.IsNullOrWhiteSpace(request.Message) ? string.Empty : request.Message;

            var config = new AuthenticationRequestConfiguration(title, message)
            {
                AllowAlternativeAuthentication = request.AllowAlternativeAuthentication,
                CancelTitle = request.CancelTitle,
                FallbackTitle = request.FallbackTitle
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(config, cancellationToken);

            if (result.Authenticated)
            {
                return AppAuthenticationResult.Success();
            }

            var status = result.Status;
            var statusName = status.ToString();
            var errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ? statusName : result.ErrorMessage;

            if (status == FingerprintAuthenticationResultStatus.Canceled)
            {
                return AppAuthenticationResult.Canceled(errorMessage);
            }

            if (status == FingerprintAuthenticationResultStatus.NotAvailable ||
                string.Equals(statusName, "NotConfigured", StringComparison.OrdinalIgnoreCase))
            {
                return AppAuthenticationResult.NotAvailable(errorMessage);
            }

            return AppAuthenticationResult.Failed(errorMessage);
        }
#endif
    }
}
