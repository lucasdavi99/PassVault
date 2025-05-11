using PassVault.Models;
using SQLite;

namespace PassVault.Data
{
    public class FolderDatabase
    {
        private SQLiteAsyncConnection? _database;

        async Task Init()
        {
            if (_database != null)
                return;
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await _database.CreateTableAsync<Folder>();
        }

        public async Task<List<Folder>> GetFoldersAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>().ToListAsync();
        }

        public async Task<Folder> GetFolderAsync(int id)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");

            return await _database.Table<Folder>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveFolderAsync(Folder folder)
        {
            await Init();
            if (_database == null) throw new InvalidOperationException("Database not initialized");
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
            if (_database == null) throw new InvalidOperationException("Database not initialized");

            var accountsToDelete = await _database.Table<Account>().Where(a => a.FolderId == folder.Id).ToListAsync();

            foreach (var account in accountsToDelete)
            {
                await _database.DeleteAsync(account);
            }

            return await _database.DeleteAsync(folder);
        }

        public async Task<List<Folder>> GetFoldersWithAccountsAsync()
        {
            await Init();

            if (_database == null) throw new InvalidOperationException("Database not initialized");

            var folders = await _database.Table<Folder>().ToListAsync();

            foreach (var folder in folders)
            {
                folder.Accounts = await _database.Table<Account>().Where(a => a.FolderId == folder.Id).ToListAsync();
            }

            return folders;
        }
    }
}
