using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Dynamic;

namespace AZ_Kviz
{
    public class Database
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

        public static (string Otazka, string Odpoved)? GetQuestionById(int id)
        {
            using var connection = GetConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT QuestionText, CorrectAnswer FROM Questions WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string otazka = reader.GetString(0);
                string odpoved = reader.GetString(1);
                return (otazka, odpoved);
            }
            else
            {
                Console.WriteLine($"Otázka s ID {id} nebyla nalezena.");
                return null;
            }
        }
    }
}
