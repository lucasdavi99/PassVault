using PassVault.Models;
using SQLite;

namespace PassVault.Data
{
    public class FolderDatabase
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
                await _database.CreateTableAsync<Folder>();

                // Adicionar índices
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Folder_Title ON Folder(Title)");
                await _database.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Folder_Created ON Folder(Created)");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Folder>> GetFoldersAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>()
                .OrderBy(f => f.Title)
                .ToListAsync();
        }

        // Nova versão com paginação
        public async Task<List<Folder>> GetFoldersPagedAsync(int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>()
                .OrderBy(f => f.Title)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Folder> GetFolderAsync(int id)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveFolderAsync(Folder folder)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            if (folder.Id != 0)
            {
                return await _database.UpdateAsync(folder);
            }
            else
            {
                return await _database.InsertAsync(folder);
            }
        }

        public async Task<int> DeleteFolderAsync(Folder folder)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            // Usar transação para garantir consistência
            var result = 0;
            await _database.RunInTransactionAsync(tran =>
            {
                // Primeiro, deletar todas as contas da pasta
                tran.Execute("DELETE FROM Account WHERE FolderId = ?", folder.Id);

                // Depois, deletar a pasta
                result = tran.Delete(folder);
            });

            return result;
        }

        // Versão otimizada - não carrega todas as contas, apenas conta
        public async Task<List<Folder>> GetFoldersWithAccountCountAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var folders = await _database.Table<Folder>()
                .OrderBy(f => f.Title)
                .ToListAsync();

            // Query mais eficiente para contar contas por pasta
            var accountCounts = await _database.QueryAsync<FolderAccountCount>(
                @"SELECT FolderId, COUNT(*) as AccountCount 
                  FROM Account 
                  WHERE FolderId IS NOT NULL 
                  GROUP BY FolderId");

            var countDict = accountCounts.ToDictionary(x => x.FolderId, x => x.AccountCount);

            // Se precisar do número de contas, pode adicionar uma propriedade
            foreach (var folder in folders)
            {
                // folder.AccountCount = countDict.GetValueOrDefault(folder.Id, 0);
            }

            return folders;
        }

        // Manter método original para compatibilidade, mas otimizado
        public async Task<List<Folder>> GetFoldersWithAccountsAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var folders = await _database.Table<Folder>()
                .OrderBy(f => f.Title)
                .ToListAsync();

            // Carregar contas de forma mais eficiente
            var allAccounts = await _database.Table<Account>()
                .Where(a => a.FolderId != null)
                .OrderBy(a => a.Title)
                .ToListAsync();

            var accountsByFolder = allAccounts.GroupBy(a => a.FolderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var folder in folders)
            {
                folder.Accounts = accountsByFolder.GetValueOrDefault(folder.Id, new List<Account>());
            }

            return folders;
        }

        // Buscar pastas por nome
        public async Task<List<Folder>> SearchFoldersByNameAsync(string name, int skip = 0, int take = 50)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            var searchTerm = $"%{name.ToLower()}%";

            return await _database.QueryAsync<Folder>(
                "SELECT * FROM Folder WHERE LOWER(Title) LIKE ? ORDER BY Title LIMIT ? OFFSET ?",
                searchTerm, take, skip);
        }

        // Contar total de pastas
        public async Task<int> GetFoldersCountAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>().CountAsync();
        }

        public void Dispose()
        {
            _database?.CloseAsync();
            _semaphore?.Dispose();
        }
    }

    // Classe auxiliar para query de contagem
    public class FolderAccountCount
    {
        public int FolderId { get; set; }
        public int AccountCount { get; set; }
    }
}