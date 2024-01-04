using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using XelerateAvalonia.Models;

namespace XelerateAvalonia.Services
{
    public class DBAccess
    {
        public static IList<CoreMeta> GetAllCoreMetas(string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            IList<CoreMeta> coreMetas = new List<CoreMeta>();
            string statement =
                "SELECT * " +
                "FROM MetaTable";

            using (cf.Connection)
            {
                SqliteCommand selectCommand = new SqliteCommand(statement, cf.Connection);
                cf.Connection.Open();
                SqliteDataReader sdr = selectCommand.ExecuteReader();

                while (sdr.Read())
                {
                    string name = sdr.GetString("Name");
                    string id = sdr.GetString("ID");
                    string deviceUsed = sdr.GetString("DeviceUsed");
                    string inputSource = sdr.GetString("InputSource");
                    float measuredTime = sdr.GetFloat("MeasuredTime");
                    float voltage = sdr.GetFloat("Voltage");
                    float current = sdr.GetFloat("Current");
                    float size = sdr.GetFloat("Size");
                    DateTime uploaded = sdr.GetDateTime("Uploaded");

                    // Creating CoreMeta using the constructor
                    CoreMeta coreMeta = new CoreMeta(name, new UniqueId(id), deviceUsed, inputSource, measuredTime, voltage, current, size, uploaded);

                    coreMetas.Add(coreMeta);
                }
            }

            return coreMetas;
        }

        public static long GetNextId(string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            string statement =
                "SELECT MAX(Id) " +
                "FROM MetaTable";

            using (cf.Connection)
            {
                SqliteCommand getMaxIdCommand = new SqliteCommand(statement, cf.Connection);
                cf.Connection.Open();
                return Convert.ToInt64(getMaxIdCommand.ExecuteScalar()) + 1;
            }
        }

        public static void SaveCoreMeta(CoreMeta coreMeta, bool isUpdate, string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            string statement;

            if (!isUpdate)
            {
                statement =
                    "INSERT INTO MetaTable " +
                    "VALUES(@Id, @Name, @DeviceUsed, @InputSource, @MeasuredTime, @Voltage, @Current, @Size, @Uploaded)";
            }
            else
            {
                statement =
                    "UPDATE MetaTable " +
                    "SET Name = @Name, DeviceUsed = @DeviceUsed, InputSource = @InputSource, MeasuredTime = @MeasuredTime, Voltage = @Voltage, Current = @Current, Size = @Size, Uploaded = @Uploaded " +
                    "WHERE Id = @Id";
            }
            using (cf.Connection)
            {
                SqliteCommand insertCommand = new SqliteCommand(statement, cf.Connection);
                cf.Connection.Open();
                insertCommand.Parameters.AddWithValue("@Id", coreMeta.ID);
                insertCommand.Parameters.AddWithValue("@Name", coreMeta.Name);
                insertCommand.Parameters.AddWithValue("@DeviceUsed", coreMeta.DeviceUsed);
                insertCommand.Parameters.AddWithValue("@InputSource", coreMeta.InputSource);
                insertCommand.Parameters.AddWithValue("@MeasuredTime", coreMeta.MeasuredTime);
                insertCommand.Parameters.AddWithValue("@Voltage", coreMeta.Voltage);
                insertCommand.Parameters.AddWithValue("@Current", coreMeta.Current);
                insertCommand.Parameters.AddWithValue("@Size", coreMeta.Size);
                insertCommand.Parameters.AddWithValue("@Uploaded", coreMeta.Uploaded);
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
                SqliteCommand deleteCommand = new SqliteCommand(statement, cf.Connection);
                deleteCommand.Parameters.AddWithValue("@Id", coreMeta.ID);
                cf.Connection.Open();
                deleteCommand.ExecuteNonQuery();
            }
        }
    }
}
