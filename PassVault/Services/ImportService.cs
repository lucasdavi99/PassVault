using System.Security.Cryptography;
using System.Text.Json;
using PassVault.Data;
using PassVault.exceptions;
using PassVault.Models;

namespace PassVault.Services
{
    public class ImportService
    {
        private readonly AccountDatabase _accountDatabase;
        private readonly FolderDatabase _folderDatabase;

        public ImportService(AccountDatabase accountDatabase, FolderDatabase folderDatabase)
        {
            _accountDatabase = accountDatabase;
            _folderDatabase = folderDatabase;
        }

        public async Task<BackupData> ImportBackupAsync(string filePath, string password)
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(filePath);

                byte[] salt = fileData.Take(16).ToArray();
                byte[] iv = fileData.Skip(16).Take(16).ToArray();
                byte[] cipherText = fileData.Skip(32).ToArray();

                using var keyDerivation = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                byte[] key = keyDerivation.GetBytes(32);

                string json;
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using var ms = new MemoryStream(cipherText);
                    using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                    using var sr = new StreamReader(cs);
                    json = sr.ReadToEnd();
                }

                var backupData = JsonSerializer.Deserialize<BackupData>(json) ?? throw new Exception("Erro ao desserializar o backup.");
                TimeSpan validade = TimeSpan.FromHours(24);
                if (DateTime.UtcNow - backupData.ExportDate > validade)
                    throw new Exception("O backup expirou.");

                await CheckImported(backupData);

                return backupData;
            }
            catch (CryptographicException)
            {
                throw new InvalidImportPasswordException();
            }
        }

        private async Task CheckImported(BackupData backupData)
        {
            foreach (var account in backupData.Accounts)
            {
                var existingAccount = await _accountDatabase.GetAccountAsync(account.Id);
                if (existingAccount != null)
                    throw new Exception("Já existe uma conta com o mesmo ID.");
            }

            foreach (var folder in backupData.Folders)
            {
                var existingFolder = await _folderDatabase.GetFolderAsync(folder.Id);
                if (existingFolder != null)
                    throw new Exception("Já existe uma pasta com o mesmo ID.");
            }
        }
    }
}
