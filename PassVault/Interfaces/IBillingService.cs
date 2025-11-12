using PassVault.Services.Billing;
using System.Threading;
using System.Threading.Tasks;

namespace PassVault.Interfaces
{
    public interface IBillingService
    {
        Task<BillingResult> PurchaseVipAsync(CancellationToken cancellationToken = default);
        Task<BillingResult> RestoreVipAsync(CancellationToken cancellationToken = default);
        Task ValidateVipStatusAsync(CancellationToken cancellationToken = default);
    }
}
