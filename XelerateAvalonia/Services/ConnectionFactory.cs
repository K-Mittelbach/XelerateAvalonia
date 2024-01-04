using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace XelerateAvalonia.Services
{
    public class ConnectionFactory
    {
        public SqliteConnection Connection { get; }

        public ConnectionFactory(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                // Create an empty database file
                using (File.Create(databasePath)) { }
            }

            Connection = new SqliteConnection($"Data Source={databasePath}");
        }
    }
}
