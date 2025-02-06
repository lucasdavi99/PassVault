﻿using SQLite;

namespace PassVault.Models
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
    }
}
