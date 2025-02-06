using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassVault.Models
{
    class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
    }
}
