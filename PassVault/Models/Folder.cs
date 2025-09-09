using SQLite;

namespace PassVault.Models
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Color { get; set; }

        public int? ParentFolderId { get; set; }

        [Ignore]
        public List<Account> Accounts { get; set; } = new List<Account>();

        [Ignore]
        public List<Folder> SubFolders { get; set; } = new List<Folder>();
    }
}