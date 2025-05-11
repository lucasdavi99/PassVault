namespace PassVault.Models
{
    public class BackupData
    {
        public DateTime ExportDate { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Folder> Folders { get; set; }
    }
}
