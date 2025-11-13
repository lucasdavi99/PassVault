using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Plugin.InAppBilling;
using OrionVault.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrionVault.Services.Billing
{
    public class BillingService : IBillingService
    {
        private const string VipProductId = "orionvault_premium";
        private readonly IVipService _vipService;
        private readonly ILogger<BillingService> _logger;

        public BillingService(IVipService vipService, ILogger<BillingService> logger)
        {
            _vipService = vipService;
            _logger = logger;
        }

        public async Task<BillingResult> PurchaseVipAsync(CancellationToken cancellationToken = default)
        {
            if (!IsBillingSupported())
            {
                return BillingResult.FromStatus(BillingResultStatus.NotSupported);
            }

            var billing = CrossInAppBilling.Current;

            try
            {
                if (!await billing.ConnectAsync(enablePendingPurchases: true, cancellationToken).ConfigureAwait(false))
                {
                    return BillingResult.FromStatus(BillingResultStatus.BillingUnavailable);
                }

                var purchase = await billing.PurchaseAsync(VipProductId, ItemType.InAppPurchase, cancellationToken: cancellationToken)
                                            .ConfigureAwait(false);

                if (purchase == null)
                {
                    return BillingResult.FromStatus(BillingResultStatus.UserCancelled);
                }

                switch (purchase.State)
                {
                    case PurchaseState.Purchased:
                    case PurchaseState.Restored:
                        PersistVipStatus(purchase);
                        return BillingResult.FromStatus(BillingResultStatus.Success);
                    case PurchaseState.PaymentPending:
                        return BillingResult.FromStatus(BillingResultStatus.Pending);
                    default:
                        return BillingResult.FromStatus(BillingResultStatus.UnknownError);
                }
            }
            catch (InAppBillingPurchaseException ex)
            {
                _logger.LogWarning(ex, "Erro durante a compra do produto VIP: {Error}", ex.PurchaseError);
                return await HandlePurchaseExceptionAsync(ex, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante a compra do produto VIP.");
                return BillingResult.FromStatus(BillingResultStatus.UnknownError, ex.Message);
            }
            finally
            {
                await billing.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<BillingResult> RestoreVipAsync(CancellationToken cancellationToken = default)
        {
            if (!IsBillingSupported())
            {
                return BillingResult.FromStatus(BillingResultStatus.NotSupported);
            }

            var billing = CrossInAppBilling.Current;

            try
            {
                if (!await billing.ConnectAsync(enablePendingPurchases: true, cancellationToken).ConfigureAwait(false))
                {
                    return BillingResult.FromStatus(BillingResultStatus.BillingUnavailable);
                }

                var purchases = await billing
                    .GetPurchasesAsync(ItemType.InAppPurchase, cancellationToken)
                    .ConfigureAwait(false);

                var vipPurchase = purchases?.FirstOrDefault(IsValidVipPurchase);

                if (vipPurchase != null)
                {
                    PersistVipStatus(vipPurchase);
                    return BillingResult.FromStatus(BillingResultStatus.Success);
                }

                if (_vipService.IsUserVip())
                {
                    _vipService.SetUserVipStatus(false);
                }

                return BillingResult.FromStatus(BillingResultStatus.NoPurchasesFound);
            }
            catch (InAppBillingPurchaseException ex)
            {
                _logger.LogWarning(ex, "Erro durante a restauração do produto VIP: {Error}", ex.PurchaseError);
                return MapError(ex.PurchaseError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado durante a restauração do produto VIP.");
                return BillingResult.FromStatus(BillingResultStatus.UnknownError, ex.Message);
            }
            finally
            {
                await billing.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task ValidateVipStatusAsync(CancellationToken cancellationToken = default)
        {
            if (!IsBillingSupported())
            {
                return;
            }

            var billing = CrossInAppBilling.Current;

            try
            {
                if (!await billing.ConnectAsync(enablePendingPurchases: true, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                var purchases = await billing
                    .GetPurchasesAsync(ItemType.InAppPurchase, cancellationToken)
                    .ConfigureAwait(false);

                var vipPurchase = purchases?.FirstOrDefault(IsValidVipPurchase);

                if (vipPurchase != null)
                {
                    PersistVipStatus(vipPurchase);
                }
                else if (_vipService.IsUserVip())
                {
                    _vipService.SetUserVipStatus(false);
                }
            }
            catch (InAppBillingPurchaseException ex)
            {
                _logger.LogWarning(ex, "Erro ao validar automaticamente o status VIP: {Error}", ex.PurchaseError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao validar automaticamente o status VIP.");
            }
            finally
            {
                await billing.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static bool IsBillingSupported()
        {
            if (DeviceInfo.Current.Platform != DevicePlatform.Android)
            {
                return false;
            }

            return CrossInAppBilling.IsSupported;
        }

        private BillingResult MapError(PurchaseError error)
        {
            return error switch
            {
                PurchaseError.BillingUnavailable or PurchaseError.AppStoreUnavailable =>
                    BillingResult.FromStatus(BillingResultStatus.BillingUnavailable),
                PurchaseError.ServiceUnavailable =>
                    BillingResult.FromStatus(BillingResultStatus.NetworkError),
                PurchaseError.InvalidProduct or PurchaseError.ItemUnavailable or PurchaseError.ProductRequestFailed =>
                    BillingResult.FromStatus(BillingResultStatus.ProductNotFound),
                PurchaseError.AlreadyOwned =>
                    BillingResult.FromStatus(BillingResultStatus.AlreadyOwned),
                PurchaseError.UserCancelled =>
                    BillingResult.FromStatus(BillingResultStatus.UserCancelled),
                PurchaseError.NotOwned =>
                    BillingResult.FromStatus(BillingResultStatus.NoPurchasesFound),
                _ => BillingResult.FromStatus(BillingResultStatus.UnknownError)
            };
        }

        private async Task<BillingResult> HandlePurchaseExceptionAsync(InAppBillingPurchaseException exception, CancellationToken cancellationToken)
        {
            if (exception.PurchaseError == PurchaseError.AlreadyOwned)
            {
                var restoreResult = await RestoreVipAsync(cancellationToken).ConfigureAwait(false);
                return restoreResult.Status == BillingResultStatus.Success
                    ? BillingResult.FromStatus(BillingResultStatus.AlreadyOwned)
                    : restoreResult;
            }

            return MapError(exception.PurchaseError);
        }

        private void PersistVipStatus(InAppBillingPurchase purchase)
        {
            _vipService.SetUserVipStatus(true, purchase.PurchaseToken);
        }

        private static bool IsValidVipPurchase(InAppBillingPurchase purchase)
        {
            return purchase.ProductId == VipProductId &&
                   (purchase.State == PurchaseState.Purchased || purchase.State == PurchaseState.Restored);
        }
    }
}
