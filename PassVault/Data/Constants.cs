namespace PassVault.Data
{
    public static class Constants
    {
        public const string DatabaseFileName = "passvault.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache |
            SQLite.SQLiteOpenFlags.ProtectionComplete;

        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);
    }
}
