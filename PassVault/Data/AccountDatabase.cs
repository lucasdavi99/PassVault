using PassVault.Interfaces;
using PassVault.Models;
using PassVault.Services.Security;
using SQLite;

namespace PassVault.Data
{
    public class AccountDatabase
    {
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IEncryptionService _encryptionService;

        public AccountDatabase(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        async Task Init()
        {
            if (_database != null)
                return;

            await _semaphore.WaitAsync();
            try
            {
                if (_database != null) // Double-check
                    return;

                _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
                await _database.CreateTableAsync<Account>();

                // Adicionar índices para melhor performance
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Account_Title ON Account(Title)");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Account_FolderId ON Account(FolderId)");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Account_Created ON Account(Created)");

                await EnsurePasswordsEncryptedAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var accounts = await _database.Table<Account>()
                .OrderBy(a => a.Title)
                .ToListAsync();

            return await DecryptAccountsAsync(accounts);
        }

        // Nova versão com paginação para melhor performance
        public async Task<List<Account>> GetAccountsPagedAsync(int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var accounts = await _database.Table<Account>()
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return await DecryptAccountsAsync(accounts);
        }

        // Otimizada para contas sem pasta
        public async Task<List<Account>> GetAccountsWithoutFolderAsync(int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var accounts = await _database.Table<Account>()
                .Where(a => a.FolderId == null)
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return await DecryptAccountsAsync(accounts);
        }

        public async Task<Account> GetAccountAsync(int id)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var account = await _database.Table<Account>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();

            return await DecryptAccountAsync(account);
        }

        // Verificar se já existe uma conta com o mesmo nome na mesma pasta
        public async Task<bool> AccountNameExistsAsync(string title, int? folderId, int? excludeAccountId = null)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var query = _database.Table<Account>()
                .Where(a => a.Title.ToLower() == title.ToLower() && a.FolderId == folderId);

            // Excluir a conta atual da verificação (para edição)
            if (excludeAccountId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAccountId.Value);
            }

            var existingAccount = await query.FirstOrDefaultAsync();
            return existingAccount != null;
        }

        public async Task<int> SaveAccountAsync(Account account)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var created = account.Created == default ? DateTime.UtcNow : account.Created;
            var encryptedPassword = await _encryptionService.EncryptAsync(account.Password ?? string.Empty);

            var dataAccount = new Account
            {
                Id = account.Id,
                Title = account.Title,
                Username = account.Username,
                Email = account.Email,
                Password = encryptedPassword,
                Created = created,
                Color = account.Color,
                FolderId = account.FolderId
            };

            int result;
            if (dataAccount.Id != 0)
            {
                result = await _database.UpdateAsync(dataAccount);
            }
            else
            {
                result = await _database.InsertAsync(dataAccount);
                account.Id = dataAccount.Id;
                account.Created = created;
            }

            return result;
        }

        public async Task<int> DeleteAccountAsync(Account account)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.DeleteAsync(account);
        }

        public async Task<List<Account>> GetAccountsByFolderIdAsync(int folderId, int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var accounts = await _database.Table<Account>()
                .Where(a => a.FolderId == folderId)
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return await DecryptAccountsAsync(accounts);
        }

        // Otimizada com LIKE index-friendly
        public async Task<List<Account>> SearchAccountsByNameAsync(string name, int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var searchTerm = $"%{name.ToLower()}%";

            var accounts = await _database.QueryAsync<Account>(
                "SELECT * FROM Account WHERE LOWER(Title) LIKE ? ORDER BY Title LIMIT ? OFFSET ?",
                searchTerm, take, skip);

            return await DecryptAccountsAsync(accounts);
        }

        // Novo método para contar total de registros
        public async Task<int> GetAccountsCountAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Account>().CountAsync();
        }

        // --- NOVO MÉTODO PARA A LÓGICA VIP ---
        public async Task<int> GetTotalAccountsAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            return await _database.Table<Account>().CountAsync();
        }

        // Novo método para contar por pasta
        public async Task<int> GetAccountsCountByFolderAsync(int? folderId)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            if (folderId.HasValue)
                return await _database.Table<Account>().Where(a => a.FolderId == folderId).CountAsync();
            else
                return await _database.Table<Account>().Where(a => a.FolderId == null).CountAsync();
        }

        // Método para limpeza e otimização do banco
        public async Task OptimizeDatabaseAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            await _database.ExecuteAsync("VACUUM");
            await _database.ExecuteAsync("ANALYZE");
        }

        public void Dispose()
        {
            _database?.CloseAsync();
            _semaphore?.Dispose();
        }

        private async Task<Account?> DecryptAccountAsync(Account? account)
        {
            if (account == null)
                return null;

            account.Password = await _encryptionService.DecryptAsync(account.Password);
            return account;
        }

        private async Task<List<Account>> DecryptAccountsAsync(List<Account> accounts)
        {
            foreach (var account in accounts)
            {
                account.Password = await _encryptionService.DecryptAsync(account.Password);
            }

            return accounts;
        }

        private async Task EnsurePasswordsEncryptedAsync()
        {
            if (_database == null)
                return;

            var legacyAccounts = await _database.QueryAsync<Account>(
                "SELECT * FROM Account WHERE Password IS NOT NULL AND Password != '' AND Password NOT LIKE ?",
                $"{EncryptionConstants.Prefix}%");

            foreach (var account in legacyAccounts)
            {
                var encryptedPassword = await _encryptionService.EncryptAsync(account.Password);
                account.Password = encryptedPassword;
                await _database.UpdateAsync(account);
            }
        }
    }
}
