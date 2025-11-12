using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;
using OrionVault.Interfaces;

namespace OrionVault.Services.Security
{
    public class EncryptionService : IEncryptionService
    {
        private const string StorageKey = "OrionVault_EncryptionKey";
        private const int KeySizeBytes = 32;
        private const int IvSizeBytes = 16;

        private readonly SemaphoreSlim _keySemaphore = new(1, 1);
        private byte[]? _cachedKey;

        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var key = await GetOrCreateKeyAsync();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }

            var cipherBytes = ms.ToArray();

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return EncryptionConstants.Prefix + Convert.ToBase64String(result);
        }

        public async Task<string> DecryptAsync(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!cipherText.StartsWith(EncryptionConstants.Prefix, StringComparison.Ordinal))
                return cipherText;

            var key = await GetOrCreateKeyAsync();
            var payload = Convert.FromBase64String(cipherText.Substring(EncryptionConstants.Prefix.Length));

            if (payload.Length < IvSizeBytes)
                throw new CryptographicException("Encrypted payload is too short.");

            var iv = new byte[IvSizeBytes];
            Buffer.BlockCopy(payload, 0, iv, 0, IvSizeBytes);

            var cipherBytes = new byte[payload.Length - IvSizeBytes];
            Buffer.BlockCopy(payload, IvSizeBytes, cipherBytes, 0, cipherBytes.Length);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        private async Task<byte[]> GetOrCreateKeyAsync()
        {
            if (_cachedKey != null)
                return _cachedKey;

            await _keySemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_cachedKey != null)
                    return _cachedKey;

                var storedKey = await SecureStorage.Default.GetAsync(StorageKey).ConfigureAwait(false);
                if (string.IsNullOrEmpty(storedKey))
                {
                    var newKey = RandomNumberGenerator.GetBytes(KeySizeBytes);
                    storedKey = Convert.ToBase64String(newKey);
                    await SecureStorage.Default.SetAsync(StorageKey, storedKey).ConfigureAwait(false);
                    _cachedKey = newKey;
                }
                else
                {
                    _cachedKey = Convert.FromBase64String(storedKey);
                }

                if (_cachedKey.Length != KeySizeBytes)
                    throw new CryptographicException("Invalid encryption key size.");

                return _cachedKey;
            }
            finally
            {
                _keySemaphore.Release();
            }
        }
    }
}
