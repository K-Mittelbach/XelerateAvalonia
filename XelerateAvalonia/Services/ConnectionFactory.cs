using System.Data.SQLite;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace XelerateAvalonia.Services
{
    public class ConnectionFactory
    {
        public SQLiteConnection Connection { get; }

        public ConnectionFactory(string databasePath)
        {
            SQLiteConnectionStringBuilder scsb = new SQLiteConnectionStringBuilder();

            if (!File.Exists(databasePath))
            {
                // Create the database file if it doesn't exist
                SQLiteConnection.CreateFile(databasePath);
            }

            // Set the connection string to the provided database path
            scsb.DataSource = databasePath;

            Connection = new SQLiteConnection(scsb.ConnectionString);
        }
    }
}
