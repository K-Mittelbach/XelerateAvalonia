using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using XelerateAvalonia.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Linq;

namespace XelerateAvalonia.Services
{
    public class DBAccess
    {
        public static ObservableCollection<CoreMeta> GetAllCoreMetas(string databasePath)
        {
            ObservableCollection<CoreMeta> coreMetas = new ObservableCollection<CoreMeta>();

            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Check if the MetaTable exists
                var checkTableExists = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MetaTable'", cf.Connection);
                var tableExists = (long)checkTableExists.ExecuteScalar();

                if (tableExists > 0)
                {
                    string statement = "SELECT * FROM MetaTable";

                    using (var selectCommand = new SQLiteCommand(statement, cf.Connection))
                    {
                        using (var sdr = selectCommand.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string name = sdr["Name"].ToString();
                                string id = sdr["ID"].ToString();
                                string deviceUsed = sdr["DeviceUsed"].ToString();
                                string inputSource = sdr["InputSource"].ToString();
                                float measuredTime = sdr.GetFloat(sdr.GetOrdinal("MeasuredTime"));
                                float voltage = sdr.GetFloat(sdr.GetOrdinal("Voltage"));
                                float current = sdr.GetFloat(sdr.GetOrdinal("Current"));
                                float size = sdr.GetFloat(sdr.GetOrdinal("Size"));
                                string uploaded = sdr.GetString(sdr.GetOrdinal("Uploaded"));

                                // Creating CoreMeta using the constructor
                                CoreMeta coreMeta = new CoreMeta(name, new UniqueId(id), deviceUsed, inputSource, measuredTime, voltage, current, size, DateTime.Parse(uploaded));

                                coreMetas.Add(coreMeta);
                            }
                        }
                    }
                }
            }

            return coreMetas;
        }




        public static void SaveCoreMeta(CoreMeta coreMeta, bool isUpdate, string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                var createTableStatement =
                    "CREATE TABLE IF NOT EXISTS MetaTable (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name TEXT, " +
                    "DeviceUsed TEXT, " +
                    "InputSource TEXT, " +
                    "MeasuredTime REAL, " +
                    "Voltage REAL, " +
                    "Current REAL, " +
                    "Size REAL, " +
                    "Uploaded TEXT" +
                    ")";

                var createTableCommand = new SQLiteCommand(createTableStatement, cf.Connection);
                createTableCommand.ExecuteNonQuery();

                string statement;

                if (isUpdate==false)
                {
                    statement =
                        "INSERT INTO MetaTable " +
                        "(Name, DeviceUsed, InputSource, MeasuredTime, Voltage, Current, Size, Uploaded) " +
                        "VALUES(@Name, @DeviceUsed, @InputSource, @MeasuredTime, @Voltage, @Current, @Size, @Uploaded)";
                }
                else
                {
                    statement =
                        "UPDATE MetaTable " +
                        "SET Name = @Name, DeviceUsed = @DeviceUsed, InputSource = @InputSource, " +
                        "MeasuredTime = @MeasuredTime, Voltage = @Voltage, Current = @Current, Size = @Size, Uploaded = @Uploaded " +
                        "WHERE Id = @Id";
                }

                var insertCommand = new SQLiteCommand(statement, cf.Connection);

                if (isUpdate)
                {
                    insertCommand.Parameters.AddWithValue("@Id", coreMeta.ID);
                }

                insertCommand.Parameters.AddWithValue("@Name", coreMeta.Name);
                insertCommand.Parameters.AddWithValue("@DeviceUsed", coreMeta.DeviceUsed);
                insertCommand.Parameters.AddWithValue("@InputSource", coreMeta.InputSource);
                insertCommand.Parameters.AddWithValue("@MeasuredTime", coreMeta.MeasuredTime);
                insertCommand.Parameters.AddWithValue("@Voltage", coreMeta.Voltage);
                insertCommand.Parameters.AddWithValue("@Current", coreMeta.Current);
                insertCommand.Parameters.AddWithValue("@Size", coreMeta.Size);
                insertCommand.Parameters.AddWithValue("@Uploaded", coreMeta.Uploaded.ToString());

                insertCommand.ExecuteNonQuery();
            }
        }

            public static void RemoveCoreMeta(CoreMeta coreMeta, string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            string statement =
                "DELETE FROM MetaTable " +
                "WHERE Id = @Id";

            using (cf.Connection)
            {
                SQLiteCommand deleteCommand = new SQLiteCommand(statement, cf.Connection);
                deleteCommand.Parameters.AddWithValue("@Id", coreMeta.ID);
                cf.Connection.Open();
                deleteCommand.ExecuteNonQuery();
            }
        }

        // Save a Core dataset in the database as a table 
        public static void SaveDataset(DataSet dataSet, string tableName, string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            string validTableName = tableName.Replace("-", "_");

            using (cf.Connection)
            {
                cf.Connection.Open();

                var headerRow = dataSet.Tables[0].Rows[2];
                string createTableStatement = $"CREATE TABLE IF NOT EXISTS {validTableName} (";

                // Sanitize column names from the third row for table creation
                foreach (DataColumn column in dataSet.Tables[0].Columns)
                {
                    string columnName = SanitizeColumnName(headerRow[column.ColumnName].ToString());
                    createTableStatement += $"{columnName} TEXT, ";
                }

                createTableStatement = createTableStatement.TrimEnd(',', ' '); // Remove the trailing comma and space
                createTableStatement += ")";

                var createTableCommand = new SQLiteCommand(createTableStatement, cf.Connection);
                createTableCommand.ExecuteNonQuery();

                // Prepare the INSERT INTO statement with placeholders
                string insertStatement = $"INSERT INTO {validTableName} VALUES ({string.Join(",", Enumerable.Repeat("?", dataSet.Tables[0].Columns.Count))})";

                using (var transaction = cf.Connection.BeginTransaction())
                {
                    using (var insertCommand = new SQLiteCommand(insertStatement, cf.Connection, transaction))
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows.Cast<DataRow>().Skip(3))
                        {
                            insertCommand.Parameters.Clear();

                            foreach (DataColumn column in dataSet.Tables[0].Columns)
                            {
                                insertCommand.Parameters.AddWithValue("@param" + column.ColumnName, row[column]);
                            }

                            insertCommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }


        // Method to sanitize column names (replace invalid characters)
        private static string SanitizeColumnName(string columnName)
        {
            // Replace common special characters and spaces with underscores
            char[] invalidChars = { '(', ')', '[', ']', '{', '}', ',', ';', '\'', '"', '`', '!', '@', '#', '$', '%', '^', '&', '*', '+', '=', '~', '|', '<', '>', '?', ' ' };

            foreach (char invalidChar in invalidChars)
            {
                columnName = columnName.Replace(invalidChar, '_');
            }

            return columnName;
        }


    }
}
