using System.Text.Json;
using LibraryApi.Model;
using LibraryApi.Model.GoogleBooks;

namespace LibraryApi.Services;

public class GoogleBooksService
{
    private readonly HttpClient httpClient;
    private readonly string apiKey;
    private const string BaseUrl = "https://www.googleapis.com/books/v1";

    public GoogleBooksService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        this.httpClient = httpClientFactory.CreateClient();
        this.apiKey = config["GoogleBooksApiKey"];
    }

    public async Task<List<Book>> SearchBooksAsync(string query, int maxResults)
    {
        var url = $"{BaseUrl}/volumes?q={Uri.EscapeDataString(query)}&maxResults={maxResults}&key={this.apiKey}";

        var response = await this.httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new List<Book>();
        }

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<BookSearchResponse>(json, options);

        if (result == null || result.Items == null)
        {
            return new List<Book>();
        }

        var books = new List<Book>();
        foreach (var item in result.Items)
        {
            books.Add(Book.FromVolume(item));
        }

        return books;
    }

    public async Task<Book> GetBookByIdAsync(string id)
    {
        var url = $"{BaseUrl}/volumes/{id}?key={this.apiKey}";

        var response = await this.httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var item = JsonSerializer.Deserialize<VolumeItem>(json, options);

        if (item == null)
        {
            return null;
        }

        return Book.FromVolume(item);
    }
}