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

        // Připojení k databázi
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
                // Vytvoření databáze, pokud neexistuje
                chosenConnection = connectionString2;
                string directory = Path.GetDirectoryName(dbPath2);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                CreateDatabase(chosenConnection);
            }

            return new SqliteConnection(chosenConnection);
        }

        // Vytvoření databáze
        private static void CreateDatabase(string connectionString)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ""Questions"" (
                        ""Id"" INTEGER NOT NULL,
                        ""QuestionText"" TEXT NOT NULL,
                        ""CorrectAnswer"" TEXT NOT NULL,
                        ""Shortcut"" TEXT NOT NULL,
                        ""Category"" TEXT,
                        PRIMARY KEY(""Id"" AUTOINCREMENT)
                    );

                    CREATE TABLE IF NOT EXISTS ""SubQuestions"" (
                        ""Id"" INTEGER NOT NULL,
                        ""QuestionText"" TEXT NOT NULL,
                        ""CorrectAnswer"" TEXT NOT NULL,
                        ""Category"" TEXT,
                        PRIMARY KEY(""Id"" AUTOINCREMENT)
                    );";
                    command.ExecuteNonQuery();
                }
            }
        }


        // Načtení otázky podle ID
        public static (string Otazka, string Odpoved, string Zkratka, string Kategorie)? GetQuestionById(int id)
        {
            using SqliteConnection connection = GetConnection();
            connection.Open();

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT QuestionText, CorrectAnswer, Shortcut, Category FROM Questions WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using SqliteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string otazka = reader.GetString(0);
                string odpoved = reader.GetString(1);
                string zkratka = reader.GetString(2);
                string kategorie = reader.IsDBNull(3) ? "" : reader.GetString(3);
                return (otazka, odpoved, zkratka, kategorie);
            }
            else
            {
                Console.WriteLine($"Otázka s ID {id} nebyla nalezena.");
                return null;
            }
        }

        // Načtení náhradní otázky podle ID
        public static (string Otazka, string Odpoved, string Kategorie)? GetSubQuestionById(int id)
        {
            using SqliteConnection connection = GetConnection();
            connection.Open();

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT QuestionText, CorrectAnswer, Category FROM SubQuestions WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using SqliteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string otazka = reader.GetString(0);
                string odpoved = reader.GetString(1);
                string kategorie = reader.IsDBNull(2) ? "" : reader.GetString(2);
                return (otazka, odpoved, kategorie);
            }
            else
            {
                Console.WriteLine($"Náhradní otázka s ID {id} nebyla nalezena.");
                return null;
            }
        }

        // Existuje otázka?
        public static bool GetQuestionByQuestion(string question)
        {
            using SqliteConnection connection = GetConnection();
            connection.Open();

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Questions WHERE QuestionText = @question";
            cmd.Parameters.AddWithValue("@question", question);

            long count = (long)cmd.ExecuteScalar();

            return count > 0;
        }

        // Existuje náhradní otázka?
        public static bool GetSubQuestionByQuestion(string question)
        {
            using SqliteConnection connection = GetConnection();
            connection.Open();

            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM SubQuestions WHERE QuestionText = @question";
            cmd.Parameters.AddWithValue("@question", question);

            long count = (long)cmd.ExecuteScalar();

            return count > 0;
        }

        // Načtení všech otázek
        public static List<Question> GetAllQuestions()
        {
            List<Question> seznam = new List<Question>();

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Shortcut, Category FROM Questions";
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seznam.Add(new Question
                            {
                                Id = reader.GetInt32(0),
                                Otazka = reader.GetString(1),
                                SpravnaOdpoved = reader.GetString(2),
                                Zkratka = reader.GetString(3),
                                Kategorie = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return seznam;
        }

        // Načtení všech náhradních otázek
        public static List<Question> GetAllSubQuestions()
        {
            List<Question> seznam = new List<Question>();

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Category FROM SubQuestions";
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seznam.Add(new Question
                            {
                                Id = reader.GetInt32(0),
                                Otazka = reader.GetString(1),
                                SpravnaOdpoved = reader.GetString(2),
                                Kategorie = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return seznam;
        }

        // Načtení všech otázek, podle kategorie
        public static List<Question> GetAllQuestions(string kategorie)
        {
            List<Question> seznam = new List<Question>();

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand cmd = connection.CreateCommand())
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
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seznam.Add(new Question
                            {
                                Id = reader.GetInt32(0),
                                Otazka = reader.GetString(1),
                                SpravnaOdpoved = reader.GetString(2),
                                Zkratka = reader.GetString(3),
                                Kategorie = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return seznam;
        }

        // Načtení všech náhradních otázek, podle kategorie
        public static List<Question> GetAllSubQuestions(string kategorie)
        {
            List<Question> seznam = new List<Question>();

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand cmd = connection.CreateCommand())
                {
                    if (kategorie == "Vše")
                    {
                        cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Category FROM SubQuestions";
                    }
                    else
                    {
                        cmd.CommandText = "SELECT Id, QuestionText, CorrectAnswer, Category FROM SubQuestions WHERE Category = @kategorie";
                        cmd.Parameters.AddWithValue("@kategorie", kategorie);
                    }
                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seznam.Add(new Question
                            {
                                Id = reader.GetInt32(0),
                                Otazka = reader.GetString(1),
                                SpravnaOdpoved = reader.GetString(2),
                                Kategorie = reader.IsDBNull(3) ? "" : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return seznam;
        }

        // Smazání otázky, podle ID
        public static void DeleteQuestion(int id)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Questions WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Smazání náhradní otázky, podle ID
        public static void DeleteSubQuestion(int id)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM SubQuestions WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Přidání otázky
        public static void AddQuestion(string otazka, string odpoved, string zkratka, string kategorie)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // Nejmenší volné ID
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

        // Přidání náhradní otázky
        public static void AddSubQuestion(string otazka, string odpoved, string kategorie)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // Nejmenší volné ID
                    command.CommandText = @"
                        INSERT INTO SubQuestions (Id, QuestionText, CorrectAnswer, Category)
                        VALUES (
                            COALESCE(
                                (SELECT 1 WHERE NOT EXISTS (SELECT 1 FROM SubQuestions WHERE Id = 1)),
                                (SELECT MIN(Id + 1) FROM SubQuestions WHERE (Id + 1) NOT IN (SELECT Id FROM SubQuestions)),1),
                        @otazka, @odpoved, @kategorie
                        );";
                    command.Parameters.AddWithValue("@otazka", otazka);
                    command.Parameters.AddWithValue("@odpoved", odpoved);
                    command.Parameters.AddWithValue("@kategorie", string.IsNullOrEmpty(kategorie) ? (object)DBNull.Value : kategorie);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Upravení otázky, podle ID
        public static void UpdateQuestion(int id, string otazka, string odpoved, string zkratka, string kategorie)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Questions
                        SET QuestionText = @otazka,
                            CorrectAnswer = @odpoved,
                            Shortcut = @zkratka,
                            Category = @kategorie
                        WHERE Id = @id;";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@otazka", otazka);
                    command.Parameters.AddWithValue("@odpoved", odpoved);
                    command.Parameters.AddWithValue("@zkratka", zkratka);
                    command.Parameters.AddWithValue("@kategorie", string.IsNullOrEmpty(kategorie) ? (object)DBNull.Value : kategorie);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Upravení náhradní otázky, podle ID
        public static void UpdateSubQuestion(int id, string otazka, string odpoved, string kategorie)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE SubQuestions
                        SET QuestionText = @otazka,
                            CorrectAnswer = @odpoved,
                            Category = @kategorie
                        WHERE Id = @id;";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@otazka", otazka);
                    command.Parameters.AddWithValue("@odpoved", odpoved);
                    command.Parameters.AddWithValue("@kategorie", string.IsNullOrEmpty(kategorie) ? (object)DBNull.Value : kategorie);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}