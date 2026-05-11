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

    //додати
    public async Task AddBookAsync(SavedBook book)
    {
        string sql = @"INSERT INTO books (title, author, status, rating)
                       VALUES (@title, @author, @status, @rating)";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("title", book.Title ?? "");

        command.Parameters.AddWithValue(
            "author",
            (book.Authors != null && book.Authors.Count > 0)
                ? string.Join(", ", book.Authors)
                : ""
        );

        command.Parameters.AddWithValue("status", book.Status ?? "");
        command.Parameters.AddWithValue("rating", book.Rating);

        await command.ExecuteNonQueryAsync();
    }

    //отримати список
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
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),

                Authors = reader.IsDBNull(2)
                    ? new List<string>()
                    : reader.GetString(2)
                        .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),

                Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Rating = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            });
        }

        return books;
    }

    //по ID
    public async Task<SavedBook?> GetByIdAsync(int id)
    {
        string sql = @"SELECT id, title, author, status, rating 
                       FROM books 
                       WHERE id = @id";

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
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),

                Authors = reader.IsDBNull(2)
                    ? new List<string>()
                    : reader.GetString(2)
                        .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),

                Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Rating = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            };
        }

        return null;
    }

    //оновити
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
        command.Parameters.AddWithValue("title", book.Title ?? "");

        command.Parameters.AddWithValue(
            "author",
            (book.Authors != null && book.Authors.Count > 0)
                ? string.Join(", ", book.Authors)
                : ""
        );

        command.Parameters.AddWithValue("status", book.Status ?? "");
        command.Parameters.AddWithValue("rating", book.Rating);

        await command.ExecuteNonQueryAsync();
    }

    //видалити
    public async Task DeleteBookAsync(int id)
    {
        string sql = "DELETE FROM books WHERE id = @id";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}