using PassVault.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.Data
{
    public class AccountDatabase
    {
        private SQLiteAsyncConnection? _database;

        async Task Init()
        {
            if (_database != null)
                return;
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await _database.CreateTableAsync<Account>();
        }

        public async Task<List<Account>> GetAccountsAsync()
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            
            return await _database.Table<Account>().ToListAsync();
        }

        public async Task<Account> GetAccountAsync(int id)
        {
            await Init();
            if (_database == null)
                throw new InvalidOperationException("Database not initialized");
            
            return await _database.Table<Account>().Where(i => i.Id == id).FirstOrDefaultAsync();
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
    }
}
