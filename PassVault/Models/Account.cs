using SQLite;

namespace PassVault.Models
{
    public class Account
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public int? FolderId { get; set; }
    }
}
