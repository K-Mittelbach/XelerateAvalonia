using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using XelerateAvalonia.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace XelerateAvalonia.Services
{
    public class DBAccess
    {
        // Retrieves all CoreMeta objects from the MetaTable in the specified database
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
                                CoreMeta coreMeta = new CoreMeta(name, new UniqueId(id), deviceUsed, inputSource, 
                                    measuredTime, voltage, current, size, DateOnly.Parse(uploaded),coreMetas,databasePath);

                                coreMetas.Add(coreMeta);
                            }
                        }
                    }
                }
            }

            return coreMetas;
        }

        // Retrieves all Core Images from the MetaTable in the specified database
        public static ObservableCollection<ImageCore> GetAllImages(string databasePath)
        {
            ObservableCollection<ImageCore> images = new ObservableCollection<ImageCore>();

            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Check if the ImageTable exists
                var checkTableExists = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ImageTable'", cf.Connection);
                var tableExists = (long)checkTableExists.ExecuteScalar();

                if (tableExists > 0)
                {
                    string statement = "SELECT * FROM ImageTable";

                    using (var selectCommand = new SQLiteCommand(statement, cf.Connection))
                    {
                        using (var sdr = selectCommand.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string name = sdr["Name"].ToString();
                                string id = sdr["ID"].ToString();
                                byte[] imageBytes = (byte[])sdr["ImageBytes"];
                                byte[] imageROI = (byte[])sdr["ImageROI"];
                                int CoreID = sdr["CoreID"] != DBNull.Value ? (int)sdr["CoreID"] : 1;
                                int SectionID = sdr["SectionID"] != DBNull.Value ? (int)sdr["SectionID"] : 1;
                                string imageType = sdr["ImageType"].ToString();
                                string imageWidth = sdr["ImageWidth"].ToString();
                                string imageHeight = sdr["ImageHeight"].ToString();
                                string imageROIStart = sdr["ImageROIStart"].ToString();
                                string imageROIEnd = sdr["ImageROIEnd"].ToString();
                                string imagePixelSize = sdr["ImagePixelSize"].ToString();
                                string imageOrientation = sdr["ImageOrientation"].ToString();
                                string imageMarginRight = sdr["ImageMarginRight"].ToString();
                                string imageMarginLeft = sdr["ImageMarginLeft"].ToString();
                                float size = sdr.GetFloat(sdr.GetOrdinal("Size"));
                                string uploaded = sdr.GetString(sdr.GetOrdinal("Uploaded"));

                                // Creating Image using the constructor
                                ImageCore image = new ImageCore(name, new UniqueId(id), imageBytes, 
                                    imageROI,CoreID, SectionID, imageType, imageWidth, imageHeight, imageROIStart, 
                                    imageROIEnd, imagePixelSize, imageOrientation, imageMarginRight, imageMarginLeft, size, 
                                    DateOnly.Parse(uploaded),images,databasePath);

                                images.Add(image);
                            }
                        }
                    }
                }
            }

            return images;
        }

        public static ObservableCollection<Cluster> GetAllClusters(string databasePath)
        {
            return GetAllClusters(databasePath, null);
        }

        public static ObservableCollection<Cluster> GetAllClusters(string databasePath, string coreName)
        {
            ObservableCollection<Cluster> clusters = new ObservableCollection<Cluster>();

            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Check if the ImageTable exists
                var checkTableExists = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ClusterTable'", cf.Connection);
                var tableExists = (long)checkTableExists.ExecuteScalar();

                if (tableExists > 0)
                {
                    string statement = "SELECT * FROM ClusterTable";

                    if (!string.IsNullOrEmpty(coreName))
                    {
                        statement += " WHERE Name = @CoreName";
                    }

                    using (var selectCommand = new SQLiteCommand(statement, cf.Connection))
                    {
                        if (!string.IsNullOrEmpty(coreName))
                        {
                            selectCommand.Parameters.AddWithValue("@CoreName", coreName);
                        }

                        using (var sdr = selectCommand.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                string name = sdr["Name"].ToString();
                                string id = sdr["ID"].ToString();
                                byte[] plot = (byte[])sdr["Plot"];
                                string ClusterType = sdr["ClusterType"].ToString();
                                int ClusterNumber = sdr["ClusterNumber"] != DBNull.Value ? (int)sdr["ClusterNumber"] : 1;
                                string clusterIDStr = sdr["ClusterID"].ToString();
                                int[] clusterID = clusterIDStr.Split(',').Select(int.Parse).ToArray();


                                // Creating Image using the constructor
                                Cluster cluster = new Cluster(name, new UniqueId(id), plot,
                                    ClusterType, ClusterNumber,clusterID);

                                clusters.Add(cluster);
                            }
                        }
                    }
                }
            }

            return clusters;
        }

        // Saves or update a CoreMeta object in the MetaTable as well as defining standard deviation and zero sum of elements
        public static void SaveCoreMeta(CoreMeta coreMeta, bool isUpdate, string databasePath, List<string> elements, List<string> elementSTD, List<string> elementZeroSum)
        {
            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                string[] DefaultElements = new string[] { "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne", "Na", "Mg", "Al", "Si", "P", "S",
                    "Cl", "Ar", "K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge",
                    "As", "Se", "Br", "Kr", "Rb", "Sr", "Y", "Zr", "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd",
                    "In", "Sn", "Sb", "Te", "I", "Xe", "Cs", "Ba", "La", "Ce", "Pr", "Nd", "Pm", "Sm", "Eu", "Gd",
                    "Tb", "Dy", "Ho", "Er", "Tm", "Yb", "Lu", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg",
                    "Tl", "Pb", "Bi", "Po", "At", "Rn", "Fr", "Ra", "Ac", "Th", "Pa", "U", "Np", "Pu", "Am", "Cm",
                    "Bk", "Cf", "Es", "Fm", "Md", "No", "Lr", "Rf", "Db", "Sg", "Bh", "Hs", "Mt", "Ds", "Rg", "Cn",
                    "Nh", "Fl", "Mc", "Lv", "Ts", "Og" };

                // Create table if not exists
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
                    "Uploaded TEXT";

                // Add columns for each element for standard deviation and zero sum
                foreach (string element in DefaultElements)
                {
                    createTableStatement += $", {element}_std TEXT DEFAULT '0', {element}_zero_sum TEXT DEFAULT '0'";
                }

                createTableStatement += ")";

                var createTableCommand = new SQLiteCommand(createTableStatement, cf.Connection);
                createTableCommand.ExecuteNonQuery();

                // Generate placeholder values for missing elements
                var defaultElementValues = Enumerable.Repeat("0", DefaultElements.Length).ToArray();

                string statement;

                if (!isUpdate)
                {
                    statement =
                        "INSERT INTO MetaTable " +
                        "(Name, DeviceUsed, InputSource, MeasuredTime, Voltage, Current, Size, Uploaded";

                    // Add columns for element standard deviation and zero sum to INSERT INTO statement
                    foreach (string element in elements)
                    {
                        statement += $", {element}_std, {element}_zero_sum";
                    }

                    statement += ") VALUES (@Name, @DeviceUsed, @InputSource, @MeasuredTime, @Voltage, @Current, @Size, @Uploaded";

                    // Add parameters with default values for missing elements
                    for (int i = 0; i < DefaultElements.Length; i++)
                    {
                        statement += ", @Param" + (i * 2) + ", @Param" + ((i * 2) + 1);
                    }

                    statement += ")";
                }
                else
                {
                    statement =
                        "UPDATE MetaTable " +
                        "SET Name = @Name, DeviceUsed = @DeviceUsed, InputSource = @InputSource, " +
                        "MeasuredTime = @MeasuredTime, Voltage = @Voltage, Current = @Current, Size = @Size, Uploaded = @Uploaded";

                    for (int i = 0; i < elements.Count; i++)
                    {
                        statement += $", {elements[i]}_std = @Param{(i * 2)}, {elements[i]}_zero_sum = @Param{(i * 2) + 1}";
                    }

                    statement += " WHERE Id = @Id";
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

                // Add parameter values for element standard deviation and zero sum
                for (int i = 0; i < elements.Count; i++)
                {
                    insertCommand.Parameters.AddWithValue("@Param" + (i * 2), elementSTD.Count > i ? elementSTD[i] : defaultElementValues[i]);
                    insertCommand.Parameters.AddWithValue("@Param" + ((i * 2) + 1), elementZeroSum.Count > i ? elementZeroSum[i] : defaultElementValues[i]);
                }

                insertCommand.ExecuteNonQuery();
            }
        }

        // Removes a CoreMeta object from the MetaTable and corresponding dataTable
        public static void RemoveCoreMeta(string coreMetaName, string databasePath)
        {
            
            ConnectionFactory cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Delete rows from MetaTable where the Name matches coreMetaName
                string deleteStatement =
                    "DELETE FROM MetaTable " +
                    "WHERE Name = @Name";

                SQLiteCommand deleteCommand = new SQLiteCommand(deleteStatement, cf.Connection);
                deleteCommand.Parameters.AddWithValue("@Name", coreMetaName);
                deleteCommand.ExecuteNonQuery();

                // Drop the table with the same name as coreMetaName
                string dropTableStatement =
                    $"DROP TABLE IF EXISTS {coreMetaName}";

                SQLiteCommand dropTableCommand = new SQLiteCommand(dropTableStatement, cf.Connection);
                dropTableCommand.ExecuteNonQuery();
            }
        }

        // Removes an Image from the MetaTable
        public static void RemoveImage(string imageName, string databasePath)
        {
            ConnectionFactory cf = new ConnectionFactory(databasePath);
            string statement =
                "DELETE FROM ImageTable " +
                "WHERE Name = @Name";

            using (cf.Connection)
            {
                SQLiteCommand deleteCommand = new SQLiteCommand(statement, cf.Connection);
                deleteCommand.Parameters.AddWithValue("@Name", imageName);
                cf.Connection.Open();
                deleteCommand.ExecuteNonQuery();
            }
        }

        // Image Import and saving in ImageTable
        public static void SaveImage(ImageCore image, bool isUpdate, string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                var createTableStatement =
                    "CREATE TABLE IF NOT EXISTS ImageTable (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name TEXT, " +
                    "ImageBytes BLOB, " +
                    "ImageROI BLOB, " +
                    "CoreID INT, " +
                    "SectionID INT, " +
                    "ImageType TEXT, " +
                    "ImageWidth TEXT, " +
                    "ImageHeight TEXT, " +
                    "ImageROIStart TEXT, " +
                    "ImageROIEnd TEXT, " +
                    "ImagePixelSize TEXT," +
                    "ImageOrientation TEXT," +
                    "ImageMarginRight TEXT," +
                    "ImageMarginLeft TEXT," +
                    "Size REAL, " +
                    "Uploaded TEXT" +
                    ")";

                var createTableCommand = new SQLiteCommand(createTableStatement, cf.Connection);
                createTableCommand.ExecuteNonQuery();

                string statement;

                if (!isUpdate)
                {
                    statement =
                        "INSERT INTO ImageTable " +
                        "(Name, ImageBytes, ImageROI, CoreID, SectionID, ImageType, ImageWidth, ImageHeight, ImageROIStart, ImageROIEnd, ImagePixelSize, ImageOrientation, " +
                        "ImageMarginRight, ImageMarginLeft, Size, Uploaded) " +
                        "VALUES(@Name,  @ImageBytes,@ImageROI, @CoreID, @SectionID, @ImageType, @ImageWidth, @ImageHeight, @ImageROIStart, @ImageROIEnd,@ImagePixelSize, @ImageOrientation, " +
                        "@ImageMarginRight, @ImageMarginLeft, @Size, @Uploaded)";
                }
                else
                {
                    statement =
                        "UPDATE ImageTable " +
                        "SET ImageBytes = @ImageBytes, ImageROI = @ImageROI, CoreID = @CoreID, SectionID = @SectionID, ImageType = @ImageType, ImageWidth = @ImageWidth, ImageHeight = @ImageHeight, " +
                        "ImageROIStart = @ImageROIStart, " +
                        "ImageROIEnd = @ImageROIEnd, ImagePixelSize = @ImagePixelSize, ImageOrientation = @ImageOrientation, ImageMarginRight = @ImageMarginRight, " +
                        "ImageMarginLeft = @ImageMarginLeft, Size = @Size, Uploaded = @Uploaded " +
                        "WHERE Name = @Name";

                }

                var insertCommand = new SQLiteCommand(statement, cf.Connection);
                          
                insertCommand.Parameters.AddWithValue("@Name", image.Name);
                insertCommand.Parameters.AddWithValue("@ImageBytes", image.Blob);
                insertCommand.Parameters.AddWithValue("@ImageROI", image.BlobROI);
                insertCommand.Parameters.AddWithValue("@CoreID", image.CoreID);
                insertCommand.Parameters.AddWithValue("@SectionID", image.SectionID);
                insertCommand.Parameters.AddWithValue("@ImageType", image.FileType);
                insertCommand.Parameters.AddWithValue("@ImageWidth", image.Width);
                insertCommand.Parameters.AddWithValue("@ImageHeight", image.Height);
                insertCommand.Parameters.AddWithValue("@ImageROIStart", image.ROIStart);
                insertCommand.Parameters.AddWithValue("@ImageROIEnd", image.ROIEnd);
                insertCommand.Parameters.AddWithValue("@ImagePixelSize", image.ImagePixelSize);
                insertCommand.Parameters.AddWithValue("@ImageOrientation", image.ImageOrientation);
                insertCommand.Parameters.AddWithValue("@ImageMarginRight", image.ImageMarginRight);
                insertCommand.Parameters.AddWithValue("@ImageMarginLeft", image.ImageMarginLeft);
                insertCommand.Parameters.AddWithValue("@Size", image.Size);
                insertCommand.Parameters.AddWithValue("@Uploaded", image.Uploaded.ToString());

                insertCommand.ExecuteNonQuery();
            }
        }

        // Save a created cluster as an image with type and number of clusters 
        public static void SaveCluster(Cluster cluster,bool isUpdate, string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                var createTableStatement =
                    "CREATE TABLE IF NOT EXISTS ClusterTable (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Name TEXT, " +
                    "Plot BLOB, " +
                    "ClusterType TEXT, " +
                    "ClusterNumber INT," +
                    "ClusterID TEXT" +
                    ")";

                var createTableCommand = new SQLiteCommand(createTableStatement, cf.Connection);
                createTableCommand.ExecuteNonQuery();

                // Check if a record with the same name and cluster type exists
                var checkExistingStatement = new SQLiteCommand("SELECT COUNT(*) FROM ClusterTable WHERE Name = @Name AND ClusterType = @ClusterType", cf.Connection);
                checkExistingStatement.Parameters.AddWithValue("@Name", cluster.Name);
                checkExistingStatement.Parameters.AddWithValue("@ClusterType", cluster.ClusterType);
                var existingCount = (long)checkExistingStatement.ExecuteScalar();

                string statement;

                if (existingCount > 0)
                {
                    // Update existing record
                    statement =
                        "UPDATE ClusterTable " +
                        "SET Plot = @Plot, ClusterNumber = @ClusterNumber, ClusterID = @ClusterID " +
                        "WHERE Name = @Name AND ClusterType = @ClusterType";
                }
                else
                {
                    // Insert new record
                    statement =
                        "INSERT INTO ClusterTable " +
                        "(Name, Plot, ClusterType, ClusterNumber, ClusterID) " +
                        "VALUES(@Name, @Plot, @ClusterType, @ClusterNumber, @ClusterID)";
                }

                var insertCommand = new SQLiteCommand(statement, cf.Connection);

                insertCommand.Parameters.AddWithValue("@Name", cluster.Name);
                insertCommand.Parameters.AddWithValue("@Plot", cluster.ClusterPlot);
                insertCommand.Parameters.AddWithValue("@ClusterType", cluster.ClusterType);
                insertCommand.Parameters.AddWithValue("@ClusterNumber", cluster.ClusterNumbers);

                // Convert the int array to a comma-separated string
                string clusterIDStr = string.Join(",", cluster.ClusterID);
                insertCommand.Parameters.AddWithValue("@ClusterID", clusterIDStr);

                insertCommand.ExecuteNonQuery();
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

        // Retrieves the count of uploaded datasets
        public static string GetUploadedFileCounts(string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                int totalCount = 0;

                // Check if the MetaTable exists
                var checkMetaTableExists = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MetaTable'", cf.Connection);
                var metaTableExists = (long)checkMetaTableExists.ExecuteScalar();

                if (metaTableExists > 0)
                {
                    var countMetaStatement = new SQLiteCommand("SELECT COUNT(*) FROM MetaTable", cf.Connection);
                    totalCount += Convert.ToInt32(countMetaStatement.ExecuteScalar());
                }

                // Check if the ImageTable exists
                var checkImageTableExists = new SQLiteCommand("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ImageTable'", cf.Connection);
                var imageTableExists = (long)checkImageTableExists.ExecuteScalar();

                if (imageTableExists > 0)
                {
                    var countImageStatement = new SQLiteCommand("SELECT COUNT(*) FROM ImageTable", cf.Connection);
                    totalCount += Convert.ToInt32(countImageStatement.ExecuteScalar());
                }

                return "(" + totalCount.ToString() +")";
            }
        }

        // Retrieves all elements a specified dataset yields information of
        public static ObservableCollection<NaturalElements> GetAllElements(string DataSetname, string databasePath)
        {
            ObservableCollection<NaturalElements> elementsCollection = new ObservableCollection<NaturalElements>();
            string[] DefaultElements = new string[] { "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne", "Na", "Mg", "Al", "Si", "P", "S",
            "Cl", "Ar", "K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge",
            "As", "Se", "Br", "Kr", "Rb", "Sr", "Y", "Zr", "Nb", "Mo", "Tc", "Ru", "Rh", "Pd", "Ag", "Cd",
            "In", "Sn", "Sb", "Te", "I", "Xe", "Cs", "Ba", "La", "Ce", "Pr", "Nd", "Pm", "Sm", "Eu", "Gd",
            "Tb", "Dy", "Ho", "Er", "Tm", "Yb", "Lu", "Hf", "Ta", "W", "Re", "Os", "Ir", "Pt", "Au", "Hg",
            "Tl", "Pb", "Bi", "Po", "At", "Rn", "Fr", "Ra", "Ac", "Th", "Pa", "U", "Np", "Pu", "Am", "Cm",
            "Bk", "Cf", "Es", "Fm", "Md", "No", "Lr", "Rf", "Db", "Sg", "Bh", "Hs", "Mt", "Ds", "Rg", "Cn",
            "Nh", "Fl", "Mc", "Lv", "Ts", "Og"};

            var cf = new ConnectionFactory(databasePath);
            using (cf.Connection)
            {
                cf.Connection.Open();

                // Define the SQL query to retrieve non-null values for the specified dataset
                string query = $"SELECT * FROM MetaTable WHERE Name = @DataSetname";
                using (var command = new SQLiteCommand(query, cf.Connection))
                {
                    command.Parameters.AddWithValue("@DataSetname", DataSetname);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Read values from the reader for each element and create NaturalElements objects
                            foreach (var element in DefaultElements)
                            {
                                string name = element.ToString();
                                UniqueId id = new UniqueId(Guid.NewGuid()); // Generate a new unique ID for each element
                                string standardDeviation = reader[$"{element}_std"].ToString();
                                string zeroSum = reader[$"{element}_zero_sum"].ToString();
                                string isChecked = "False";

                                // Skip if standard deviation is null
                                if (string.IsNullOrEmpty(standardDeviation))
                                {
                                    continue;
                                }

                                // Create NaturalElements object and add it to the collection
                                NaturalElements elements = new NaturalElements(name, id, standardDeviation, zeroSum, isChecked);
                                elementsCollection.Add(elements);
                            }
                        }
                    }
                }
            }

            return elementsCollection;
        }
        public static void UpdateAndDeleteColumns(string dataSetName, List<string> elementNames,string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);
            using (cf.Connection)
            {
                cf.Connection.Open();

                // Update the ElementValues in the MetaTable
                UpdateMetaTable(dataSetName, elementNames, cf.Connection);

                // Delete columns from the table with the DataSetName
                DeleteColumnsFromTable(dataSetName, elementNames, cf.Connection);
            }
        }

        private static void UpdateMetaTable(string dataSetName, List<string> elementNames, SQLiteConnection connection)
        {
            foreach (var elementName in elementNames)
            {
                // Construct the column names
                string stdColumnName = $"{elementName}_std";
                string zeroSumColumnName = $"{elementName}_zero_sum";

                // Update the ElementValues to null in the MetaTable
                var updateCmd = new SQLiteCommand($"UPDATE MetaTable SET {stdColumnName} = NULL, {zeroSumColumnName} = NULL WHERE Name = @DataSetName", connection);
                updateCmd.Parameters.AddWithValue("@DataSetName", dataSetName);
                updateCmd.ExecuteNonQuery();
            }
        }

        private static void DeleteColumnsFromTable(string dataSetName, List<string> elementNames, SQLiteConnection connection)
        {
            foreach (var elementName in elementNames)
            {
                // Construct the column names
                string columnName = $"{elementName}";

                // Check if the column exists in the table and delete it if it does
                DeleteColumnIfExists(columnName, dataSetName, connection);
            }
        }

        private static void DeleteColumnIfExists(string columnName, string tableName, SQLiteConnection connection)
        {
            // Construct the SQL statement to check if the column exists in the specified table
            string statement = $"PRAGMA table_info({tableName})";

            using (var command = new SQLiteCommand(statement, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Get the name of each column in the table
                        string existingColumnName = reader["name"].ToString();

                        // If the column name matches the one to be deleted, drop the column
                        if (existingColumnName == columnName)
                        {
                            // If the column exists, construct the SQL command to drop it
                            string dropColumnStatement = $"ALTER TABLE {tableName} DROP COLUMN {columnName}";
                            var dropColumnCmd = new SQLiteCommand(dropColumnStatement, connection);
                            dropColumnCmd.ExecuteNonQuery();
                            break; // Exit the loop after dropping the column
                        }
                    }
                }
            }
        }


        // Retrieves all core sections a specified dataset yields information of
        public static ObservableCollection<CoreSections> GetAllCoreSections(string tableName, string databasePath)
        {
            ObservableCollection<CoreSections> coreSectionsList = new ObservableCollection<CoreSections>();

            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Using parameterized query to prevent SQL injection
                string query = $"SELECT CoreID, SectionID FROM {tableName} " +
                               "ORDER BY CAST(SUBSTR(CoreID, 5) AS INTEGER), " +
                               "CAST(SUBSTR(SectionID, 9) AS INTEGER)";

                using (var command = new SQLiteCommand(query, cf.Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        string currentCoreId = null;
                        string currentSectionId = null;
                        int startRow = 1; // Initial start row
                        int endRow = 0;   // Initial end row
                        string isChecked = "True";
                        string hasImage = "False";
                        byte[] image = null;
                        bool firstIteration = true; // Flag to indicate first iteration

                        while (reader.Read())
                        {
                            double coreId = Math.Round(Convert.ToDouble(reader["CoreID"]), 1);
                            double sectionId = Math.Round(Convert.ToDouble(reader["SectionID"]), 1);

                            string coreName = "Core " + coreId.ToString();
                            string sectionName = "Section " + sectionId.ToString();

                            if (firstIteration)
                            {
                                startRow = 1; // Initialize startRow only on the first iteration
                                firstIteration = false; // Update flag
                            }

                            if (currentCoreId != coreName || currentSectionId != sectionName)
                            {
                                // Get Image for CoreSection
                                image = GetImageROIForCoreSection(tableName, coreId.ToString(), sectionId.ToString(), databasePath);
                                hasImage = (image != null) ? "True" : "False"; // Check if image is null

                                if (currentCoreId != null && currentSectionId != null)
                                {
                                    coreSectionsList.Add(new CoreSections(currentCoreId, currentSectionId, new UniqueId(Guid.NewGuid()), image, startRow.ToString(), (endRow - 1).ToString(), hasImage, isChecked));
                                }

                                currentCoreId = coreName;
                                currentSectionId = sectionName;

                                startRow = endRow + 1; // Update start row for the next CoreSections object
                            }

                            endRow++; // Increment end row for each CoreId
                        }

                        // Add the last CoreSection
                        if (currentCoreId != null && currentSectionId != null)
                        {
                            coreSectionsList.Add(new CoreSections(currentCoreId, currentSectionId, new UniqueId(Guid.NewGuid()), image, startRow.ToString(), (endRow - 1).ToString(), hasImage, isChecked));
                        }
                    }
                }
            }

            return coreSectionsList;
        }

        private static byte[] GetImageROIForCoreSection(string coreName, string coreId, string sectionId, string databasePath)
        {
            var cf = new ConnectionFactory(databasePath);

            coreName = coreName.Substring("Core_".Length);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Access the images from ImageTable
                string query = "SELECT ImageROI FROM ImageTable WHERE Name LIKE @coreName AND CoreID = @coreId AND SectionID = @sectionId";
                using (var command = new SQLiteCommand(query, cf.Connection))
                {
                    command.Parameters.AddWithValue("@coreName", "%" + coreName + "%");
                    command.Parameters.AddWithValue("@coreId", coreId);
                    command.Parameters.AddWithValue("@sectionId", sectionId);

                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        return (byte[])result;
                    }
                }
            }

            return null; // Return null if no ImageROI found
        }

        // Retrieves a dataTable object from a saved dataset 
        public static DataTable GetTableAsDataTable(string tableName, string databasePath)
        {
            DataTable dataTable = new DataTable(tableName);

            var cf = new ConnectionFactory(databasePath);

            using (cf.Connection)
            {
                cf.Connection.Open();

                // Using parameterized query to prevent SQL injection
                string query = $"SELECT * FROM {tableName}";

                using (var command = new SQLiteCommand(query, cf.Connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        // Populate the DataTable with column names and types
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);

                            // Exclude specific columns
                            if (columnName != "filename" &&
                                columnName != "Voltage" &&
                                columnName != "Amperage" &&
                                columnName != "CoreID" &&
                                columnName != "SectionID" &&
                                columnName != "SampleID" &&
                                columnName != "RepID" &&
                                columnName != "cps" &&
                                columnName != "Dt" &&
                                columnName != "MSE")
                            {
                                dataTable.Columns.Add(columnName, reader.GetFieldType(i));
                            }
                        }

                        // Populate the DataTable with data
                        while (reader.Read())
                        {
                            DataRow row = dataTable.NewRow();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);

                                // Exclude specific columns
                                if (columnName != "filename" &&
                                    columnName != "Voltage" &&
                                    columnName != "Amperage" &&
                                    columnName != "CoreID" &&
                                    columnName != "SectionID" &&
                                    columnName != "SampleID" &&
                                    columnName != "RepID" &&
                                    columnName != "cps" &&
                                    columnName != "Dt" &&
                                    columnName != "MSE")
                                {
                                    row[columnName] = reader[i];
                                }
                            }

                            dataTable.Rows.Add(row);
                        }
                    }
                }
            }

            return dataTable;
        }




    }
}
