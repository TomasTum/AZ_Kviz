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
        // Cesta k databázi ve Visual Studiu
        private static string dbPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data", "azkviz.db");
        // Cesta k databázi v publikované aplikaci
        private static string dbPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "azkviz.db");

        private static string connectionString1 => $"Data Source={Path.GetFullPath(dbPath1)}";
        private static string connectionString2 => $"Data Source={Path.GetFullPath(dbPath2)}";

        // Získání připojení k databázi
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


        // Načtení otázky podle ID, filtrování podle kategorie
        public static (string Otazka, string Odpoved, string Zkratka)? GetQuestionById(int id)
        {
            using var connection = GetConnection();
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT QuestionText, CorrectAnswer, Shortcut, Category FROM Questions WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string otazka = reader.GetString(0);
                string odpoved = reader.GetString(1);
                string zkratka = reader.GetString(2);
                return (otazka, odpoved, zkratka);
            }
            else
            {
                Console.WriteLine($"Otázka s ID {id} nebyla nalezena.");
                return null;
            }
        }

        // Načtení všech otázek
        public static List<Question> GetAllQuestions()
        {
            var seznam = new List<Question>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Shortcut, Category FROM Questions";

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
                                Zkratka = reader.GetString(3),
                                // Ošetření, kdyby kategorie byla prázdná (null)
                                Kategorie = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return seznam;
        }

        // Načtení všech otázek, filtrovaných podle kategorie
        public static List<Question> GetAllQuestions(string kategorie)
        {
            var seznam = new List<Question>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    if (kategorie == "Vše")
                    {
                        cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Shortcut, Category FROM Questions";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Shortcut, Category FROM Questions WHERE Category = @kategorie";
                        cmd.Parameters.AddWithValue("@kategorie", kategorie);
                    }

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
                                Zkratka = reader.GetString(3),
                                // Ošetření, kdyby kategorie byla prázdná (null)
                                Kategorie = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return seznam;
        }

        // Smazání otázky z databáze podle ID
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

        // Přidání nové otázky do databáze
        public static void AddQuestion(string otazka, string odpoved, string zkratka, string kategorie)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    //Zjistíme nejmenší volné ID pro novou otázku
                    command.CommandText = @"
                        INSERT INTO Questions (Id, QuestionText, CorrectAnswer, Shortcut, Category)
                        VALUES (
                            COALESCE(
                                (SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM Questions WHERE Id = 1)),
                                (SELECT MIN(Id + 1) FROM Questions WHERE (Id + 1) NOT IN (SELECT Id FROM Questions)),1),
                        @otazka, @odpoved, @zkratka, @kategorie
                        );";
                    command.Parameters.AddWithValue("@otazka", otazka);
                    command.Parameters.AddWithValue("@odpoved", odpoved);
                    command.Parameters.AddWithValue("@zkratka", zkratka);
                    command.Parameters.AddWithValue("@kategorie", string.IsNullOrEmpty(kategorie) ? (object)DBNull.Value : kategorie);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
