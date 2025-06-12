using PassVault.Models;
using SQLite;

namespace PassVault.Data
{
    public class AccountDatabase
    {
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

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

            return await _database.Table<Account>()
                .OrderBy(a => a.Title)
                .ToListAsync();
        }

        // Nova versão com paginação para melhor performance
        public async Task<List<Account>> GetAccountsPagedAsync(int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Account>()
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        // Otimizada para contas sem pasta
        public async Task<List<Account>> GetAccountsWithoutFolderAsync(int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Account>()
                .Where(a => a.FolderId == null)
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Account> GetAccountAsync(int id)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Account>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveAccountAsync(Account account)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            if (account.Id != 0)
            {
                return await _database.UpdateAsync(account);
            }
            else
            {
                return await _database.InsertAsync(account);
            }
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

            return await _database.Table<Account>()
                .Where(a => a.FolderId == folderId)
                .OrderBy(a => a.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        // Otimizada com LIKE index-friendly
        public async Task<List<Account>> SearchAccountsByNameAsync(string name, int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var searchTerm = $"%{name.ToLower()}%";

            return await _database.QueryAsync<Account>(
                "SELECT * FROM Account WHERE LOWER(Title) LIKE ? ORDER BY Title LIMIT ? OFFSET ?",
                searchTerm, take, skip);
        }

        // Novo método para contar total de registros
        public async Task<int> GetAccountsCountAsync()
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
    }
}