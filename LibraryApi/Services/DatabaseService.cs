using LibraryApi.Models;
using Npgsql;
namespace LibraryApi.Services;
public class DatabaseService
{
    private readonly string connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public async Task AddBookAsync(SavedBook book)
    {
        string sql = @"INSERT INTO books (title, author, status, rating)
                       VALUES (@title, @author, @status, @rating)";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", string.Join(", ", book.Authors ?? new List<string>()));
        command.Parameters.AddWithValue("status", book.Status);
        command.Parameters.AddWithValue("rating", book.Rating);

        await command.ExecuteNonQueryAsync();
    }
    public async Task<List<SavedBook>> GetBooksAsync()
    {
        var books = new List<SavedBook>();

        string sql = @"SELECT id, title, author, status, rating 
                   FROM books
                   ORDER BY id";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            books.Add(new SavedBook
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Authors = reader.GetString(2).Split(", ").ToList(),
                Status = reader.GetString(3),
                Rating = reader.GetInt32(4)
            });
        }

        return books;
    }
    public async Task UpdateBookAsync(SavedBook book)
    {
        string sql = @"UPDATE books
                       SET title = @title,
                           author = @author,
                           status = @status,
                           rating = @rating
                       WHERE id = @id";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", book.Id);
        command.Parameters.AddWithValue("title", book.Title);
        command.Parameters.AddWithValue("author", string.Join(", ", book.Authors ?? new List<string>()));
        command.Parameters.AddWithValue("status", book.Status);
        command.Parameters.AddWithValue("rating", book.Rating);

        await command.ExecuteNonQueryAsync();
    }
    public async Task DeleteBookAsync(int id)
    {
        string sql = "DELETE FROM books WHERE id = @id";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
    public async Task<SavedBook?> GetByIdAsync(int id)
    {
        string sql = "SELECT id, title, author, status, rating FROM books WHERE id = @id";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new SavedBook
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Authors = reader.GetString(2).Split(", ").ToList(),
                Status = reader.GetString(3),
                Rating = reader.GetInt32(4)
            };
        }

        return null;
    }
}