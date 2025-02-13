using Microsoft.Maui.Controls.PlatformConfiguration;
using PassVault.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PassVault.Services
{
    public class ExportService
    {
        public async Task<(string filePath, string password)> ExportBackupAsync(List<Account> accounts, List<Folder> folders)
        {
            var backupData = new BackupData
            {
                ExportDate = DateTime.UtcNow,
                Accounts = accounts,
                Folders = folders
            };

            string json = JsonSerializer.Serialize(backupData);
            string password = GenerateRandomPassword(12);

            byte[] salt = new byte[16];
            byte[] iv = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(iv);
            }

            var keyDerivation = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(32);

            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using (var ms = new MemoryStream())
                {
                    ms.Write(salt, 0, salt.Length);
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(json);
                    }
                    encrypted = ms.ToArray();
                }
            }

            string filePath = Path.Combine(FileSystem.AppDataDirectory, "backup.dat");
            await File.WriteAllBytesAsync(filePath, encrypted);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Compartilhar Backup",
                File = new ShareFile(filePath)
            });

            return (filePath, password);
        }

        private string GenerateRandomPassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                while (res.Length < length)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }
    }
}
