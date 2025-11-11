using CommunityToolkit.Mvvm.Messaging;
using PassVault.Configuration;
using PassVault.Interfaces;
using PassVault.Messages;
using System.Security.Cryptography;
using System.Text;

namespace PassVault.Services
{
    public class VipService : IVipService
    {

        // 1. CHAVE SECRETA MESTRA (Hardcoded)
        // Mude esta chave para algo único e complexo antes de publicar!
        private const string VipSecretKey = "p@s5vauLt_s3cReT_phR@s3_f0R_v1p_!2025";

        // 2. CHAVES OFUSCADAS PARA OS PREFERENCES
        private const string PrefKey1 = "cfg_stat_a1";   // Armazenará o hash do segredo
        private const string PrefKey2 = "cfg_mode_b2";   // Armazenará um cálculo
        private const string PrefKey3 = "cfg_unlock_c3"; // Armazenará uma parte invertida do hash
        private const string PrefKey4 = "cfg_token_d4";  // Armazenará o token da compra

        public bool IsUserVip()
        {
            try
            {
                if (AppBuildSettings.ForceVipForTesting)
                {
                    return true;
                }

                // Calcula os valores esperados em tempo real
                string expectedHash = CalculateSHA256(VipSecretKey);
                string expectedLengthCalc = (VipSecretKey.Length * 7).ToString();
                string expectedInvertedPart = new string(expectedHash.Substring(0, 10).Reverse().ToArray());

                // Obtém os valores salvos no dispositivo
                string storedHash = Preferences.Get(PrefKey1, string.Empty);
                string storedLengthCalc = Preferences.Get(PrefKey2, string.Empty);
                string storedInvertedPart = Preferences.Get(PrefKey3, string.Empty);

                // Compara os 3 valores. Todos devem ser idênticos.
                return storedHash == expectedHash &&
                       storedLengthCalc == expectedLengthCalc &&
                       storedInvertedPart == expectedInvertedPart;
            }
            catch (Exception)
            {
                // Por segurança, qualquer erro na verificação retorna falso.
                return false;
            }
        }

        public void SetUserVipStatus(bool isVip, string? purchaseToken = null)
        {
            if (AppBuildSettings.ForceVipForTesting)
            {
                WeakReferenceMessenger.Default.Send(new VipStatusChangedMessage(true));
                return;
            }

            if (isVip)
            {
                // Calcula e salva o "pacote de segurança"
                string hash = CalculateSHA256(VipSecretKey);
                string lengthCalc = (VipSecretKey.Length * 7).ToString();
                string invertedHashPart = new string(hash.Substring(0, 10).Reverse().ToArray());

                Preferences.Set(PrefKey1, hash);
                Preferences.Set(PrefKey2, lengthCalc);
                Preferences.Set(PrefKey3, invertedHashPart);
                if (!string.IsNullOrWhiteSpace(purchaseToken))
                {
                    Preferences.Set(PrefKey4, purchaseToken);
                }
            }
            else
            {
                // Limpa as chaves para remover o status VIP
                Preferences.Remove(PrefKey1);
                Preferences.Remove(PrefKey2);
                Preferences.Remove(PrefKey3);
                Preferences.Remove(PrefKey4);
            }

            WeakReferenceMessenger.Default.Send(new VipStatusChangedMessage(isVip));
        }

        public string? GetVipPurchaseToken()
        {
            return Preferences.Get(PrefKey4, null);
        }

        private string CalculateSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
