using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using zadanie.Models;

namespace zadanie.Services;

public class DatabaseHelper
{
    private readonly string _connectionString;
    public DatabaseHelper(string connectionString = "Host=localhost;Port=5432;Database=TaskDB;Username=postgres;Password=postgres")
    {
        _connectionString = connectionString;
        EnsureTableExists().Wait();
    }

    private async Task EnsureTableExists()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = @"
            CREATE TABLE IF NOT EXISTS tasks (
                Id SERIAL PRIMARY KEY,
                Title VARCHAR(200) NOT NULL,
                Description TEXT,
                IsCompleted BOOLEAN NOT NULL DEFAULT FALSE
            )";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        var tasks = new List<TaskItem>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        const string sql = "SELECT Id, Title, Description, IsCompleted FROM tasks";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
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
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        const string sql = "INSERT INTO tasks (Title, Description, IsCompleted) VALUES (@Title, @Description, @IsCompleted)";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Title", task.Title);
        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(task.Description) ? DBNull.Value : (object)task.Description);
        cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        const string sql = "UPDATE tasks SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted WHERE Id = @Id";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", task.Id);
        cmd.Parameters.AddWithValue("@Title", task.Title);
        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(task.Description) ? DBNull.Value : (object)task.Description);
        cmd.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteTaskAsync(int id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        const string sql = "DELETE FROM tasks WHERE Id = @Id";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}