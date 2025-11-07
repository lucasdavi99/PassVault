namespace PassVault.Services.Security
{
    public enum AppAuthenticationStatus
    {
        Success,
        Failed,
        Canceled,
        NotAvailable,
        Error
    }

    public sealed class AppAuthenticationResult
    {
        private AppAuthenticationResult(AppAuthenticationStatus status, string? errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public AppAuthenticationStatus Status { get; }
        public string? ErrorMessage { get; }

        public bool IsSuccessful => Status == AppAuthenticationStatus.Success;

        public static AppAuthenticationResult Success() => new(AppAuthenticationStatus.Success);
        public static AppAuthenticationResult Failed(string? message = null) => new(AppAuthenticationStatus.Failed, message);
        public static AppAuthenticationResult Canceled(string? message = null) => new(AppAuthenticationStatus.Canceled, message);
        public static AppAuthenticationResult NotAvailable(string? message = null) => new(AppAuthenticationStatus.NotAvailable, message);
        public static AppAuthenticationResult Error(string? message = null) => new(AppAuthenticationStatus.Error, message);
    }

    public sealed class AppAuthenticationRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool AllowAlternativeAuthentication { get; init; } = true;
        public string? CancelTitle { get; init; }
        public string? FallbackTitle { get; init; }
    }
}
