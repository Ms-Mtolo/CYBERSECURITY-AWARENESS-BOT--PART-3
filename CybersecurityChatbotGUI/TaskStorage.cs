using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CybersecurityChatbotGUI
{
    public class TaskStorage
    {
        // CHANGE THIS TO YOUR PASSWORD!
        private string connectionString = "Server=localhost;Port=3306;Database=cybersecurity;Uid=root;Pwd=Boikhantsho@6;";

        public TaskStorage()
        {
            // Create table if it doesn't exist
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS user_tasks (
                            id INT PRIMARY KEY AUTO_INCREMENT,
                            title VARCHAR(200) NOT NULL,
                            description TEXT,
                            reminder_date DATETIME,
                            is_completed BOOLEAN DEFAULT FALSE,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        )";
                    using (var cmd = new MySqlCommand(createTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
            }
        }

        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO user_tasks (title, description, reminder_date) 
                                    VALUES (@title, @desc, @reminder)";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@reminder", reminderDate ?? (object)DBNull.Value);
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding task: {ex.Message}");
                return 0;
            }
        }

        public List<TaskItem> GetTasks(bool includeCompleted = false)
        {
            var tasks = new List<TaskItem>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = includeCompleted ?
                        "SELECT * FROM user_tasks ORDER BY is_completed ASC, reminder_date ASC" :
                        "SELECT * FROM user_tasks WHERE is_completed = FALSE ORDER BY reminder_date ASC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
                                IsCompleted = reader.GetBoolean("is_completed")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting tasks: {ex.Message}");
            }
            return tasks;
        }

        public bool MarkTaskCompleted(int taskId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE user_tasks SET is_completed = TRUE WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing task: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM user_tasks WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task: {ex.Message}");
                return false;
            }
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }

        public string DisplayText => $"[{(IsCompleted ? "✓" : "○")}] {Title} - {(ReminderDate?.ToString("yyyy-MM-dd") ?? "No reminder")}";
    }
}