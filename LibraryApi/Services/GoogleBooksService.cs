using System.Text.Json;
using LibraryApi.Models;

namespace LibraryApi.Services;
public class GoogleBooksService
{
    private HttpClient httpClient;
    public GoogleBooksService()
    {
        httpClient = new HttpClient();
    }
    //пошук за назвою
    public async Task<List<Book>> SearchBooksAsync(string query)
    {
        string url = "https://www.googleapis.com/books/v1/volumes?q=" + query;

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<Book>();

        string json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = JsonSerializer.Deserialize<GoogleBooksResponse>(json, options);
        return result?.Items ?? new List<Book>();
    }
    //по ID
    public async Task<Book> GetBookByIdAsync(string id)
    {
        string url = "https://www.googleapis.com/books/v1/volumes/" + id;

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        string json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<Book>(json, options);
    }
    //за автором
    public async Task<List<Book>> SearchByAuthorAsync(string author)
    {
        string url = "https://www.googleapis.com/books/v1/volumes?q=inauthor:" + author;

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<Book>();

        string json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var result = JsonSerializer.Deserialize<GoogleBooksResponse>(json, options);
        return result?.Items ?? new List<Book>();
    }
}