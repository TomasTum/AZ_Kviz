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
        //cesta k databázi ve visual studiu
        private static string dbPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data", "azkviz.db");
        //cesta k databázi v publikované verzi
        private static string dbPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "azkviz.db");

        private static string connectionString1 => $"Data Source={Path.GetFullPath(dbPath1)}";
        private static string connectionString2 => $"Data Source={Path.GetFullPath(dbPath2)}";

        public static SqliteConnection GetConnection()
        {
            string chosenPath;
            string chosenConnection;

            if (File.Exists(dbPath1))
            {
                chosenPath = dbPath1;
                chosenConnection = connectionString1;
            }
            else if (File.Exists(dbPath2))
            {
                chosenPath = dbPath2;
                chosenConnection = connectionString2;
            }
            else
            {
                throw new FileNotFoundException("Databázový soubor nebyl nalezen v žádné z očekávaných cest!", $"{dbPath1} nebo {dbPath2}");
            }

            return new SqliteConnection(chosenConnection);
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

        public static List<Question> GetAllQuestions()
        {
            var seznam = new List<Question>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Category FROM Questions";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seznam.Add(new Question
                            {
                                // Čísla v závorce (0, 1, 2...) odpovídají pořadí v SELECT příkazu
                                Id = reader.GetInt32(0),
                                Otazka = reader.GetString(1),
                                SpravnaOdpoved = reader.GetString(2),
                                // Ošetření, kdyby kategorie byla prázdná (null)
                                Kategorie = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return seznam;
        }

        public static void DeleteQuestion(int id)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Questions WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void AddQuestion(string otazka, string odpoved, string kategorie)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Questions (QuestionText, CorrectAnswer, Category) VALUES (@otazka, @odpoved, @kategorie)";
                    command.Parameters.AddWithValue("@otazka", otazka);
                    command.Parameters.AddWithValue("@odpoved", odpoved);
                    command.Parameters.AddWithValue("@kategorie", string.IsNullOrEmpty(kategorie) ? (object)DBNull.Value : kategorie);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
