using System;

namespace PassVault.Services.Billing
{
    public enum BillingResultStatus
    {
        Success,
        AlreadyOwned,
        UserCancelled,
        Pending,
        BillingUnavailable,
        NetworkError,
        ProductNotFound,
        NoPurchasesFound,
        NotSupported,
        UnknownError
    }

    public sealed class BillingResult
    {
        public BillingResultStatus Status { get; }
        public string? ErrorMessage { get; }

        public bool IsSuccess => Status == BillingResultStatus.Success || Status == BillingResultStatus.AlreadyOwned;

        private BillingResult(BillingResultStatus status, string? errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static BillingResult FromStatus(BillingResultStatus status, string? message = null) =>
            new(status, message);
    }
}
