using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;

namespace AZ_Kviz
{
    internal class Database
    {
        private static string dbPath = Path.Combine("Data", "azkviz.db");
        private static string connectionString = $"Data Source={dbPath}";

        public static SqliteConnection GetConnection()
        {
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Databázový soubor nebyl nalezen!", dbPath);
            }

            return new SqliteConnection(connectionString);
        }
    }
}
