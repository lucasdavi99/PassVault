using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.Interfaces
{
    public interface IVipService
    {
        bool IsUserVip();
        void SetUserVipStatus(bool isVip, string? purchaseToken = null);
        string? GetVipPurchaseToken();
    }
}
