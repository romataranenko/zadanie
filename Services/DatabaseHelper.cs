using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using zadanie.Models;

namespace zadanie.Services
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        
        public DatabaseHelper(string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=p511_db")
        {
            _connectionString = connectionString;
        }

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            var tasks = new List<TaskItem>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            string sql = "SELECT \"Id\", \"Title\", \"Description\", \"IsCompleted\" FROM \"Tasks\"";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(new TaskItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    IsCompleted = reader.GetBoolean(3)
                });
            }
            return tasks;
        }

        public async Task AddTaskAsync(TaskItem task)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            string sql = "INSERT INTO \"Tasks\" (\"Title\", \"Description\", \"IsCompleted\") VALUES (@Title, @Description, @IsCompleted)";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(task.Description) ? DBNull.Value : (object)task.Description);
            cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            string sql = "UPDATE \"Tasks\" SET \"Title\"=@Title, \"Description\"=@Description, \"IsCompleted\"=@IsCompleted WHERE \"Id\"=@Id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", task.Id);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(task.Description) ? DBNull.Value : (object)task.Description);
            cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            string sql = "DELETE FROM \"Tasks\" WHERE \"Id\"=@Id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}