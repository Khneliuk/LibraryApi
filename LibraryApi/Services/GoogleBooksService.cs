using System.Text.Json;
using LibraryApi.Models;
namespace LibraryApi.Services;
public class GoogleBooksService
{
    private readonly HttpClient _http;
    private const string ApiKey = "AIzaSyDzt8QL33RSlduBE1Gp6xLmvvy8EkGUytw"; 
    public GoogleBooksService()
    {
        _http = new HttpClient();
    }

    //знайти за назвою
    public Task<List<SavedBook>> SearchBooksAsync(string query)
    {
        var key = string.IsNullOrEmpty(ApiKey) ? "" : $"&key={ApiKey}";
        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}&maxResults=10{key}";
        Console.WriteLine($"[Google Books] Title search: {query}");
        return FetchBooks(url);
    }

    //хнайти за автором
    public Task<List<SavedBook>> SearchByAuthorAsync(string author)
    {
        var key = string.IsNullOrEmpty(ApiKey) ? "" : $"&key={ApiKey}";
        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(author)}&maxResults=10{key}";
        Console.WriteLine($"[Google Books] Author search: {author}");
        return FetchBooks(url);
    }
    public async Task<List<SavedBook>> FetchBooks(string url)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var response = await _http.GetAsync(url);
                Console.WriteLine($"[Google Books] Attempt {attempt}: HTTP {(int)response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Console.WriteLine("[Google Books] 503");
                    await Task.Delay(1000);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Google Books] Error {(int)response.StatusCode}: {err}");
                    return new List<SavedBook>();
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("totalItems", out var total))
                    Console.WriteLine($"[Google Books] totalItems: {total.GetInt32()}");

                var result = new List<SavedBook>();

                if (!doc.RootElement.TryGetProperty("items", out var items))
                {
                    Console.WriteLine("[Google Books] Немає 'items'");
                    return result;
                }

                foreach (var item in items.EnumerateArray())
                {
                    var volume = item.GetProperty("volumeInfo");

                    string title = volume.TryGetProperty("title", out var t)
                        ? t.GetString() ?? "No title"
                        : "No title";

                    var authors = new List<string>();
                    if (volume.TryGetProperty("authors", out var a))
                    {
                        authors = a.EnumerateArray()
                            .Select(x => x.GetString() ?? "")
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToList();
                    }

                    string description = volume.TryGetProperty("description", out var d)
                        ? d.GetString() ?? ""
                        : "";

                    result.Add(new SavedBook
                    {
                        Title = title,
                        Authors = authors,
                        Description = description,
                        Status = "want to read",
                        Rating = 0
                    });
                }

                Console.WriteLine($"[Google Books] Повернуто {result.Count} книг");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Google Books] Exception (attempt {attempt}): {ex.Message}");
                if (attempt == 3) return new List<SavedBook>();
                await Task.Delay(500);
            }
        }

        return new List<SavedBook>();
    }
}