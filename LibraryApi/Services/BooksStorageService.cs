using System.Text.Json;
using LibraryApi.Models;

namespace LibraryApi.Services;
public class BooksStorageService
{
    private string filePath = "Storage/books.json";
    public List<SavedBook> GetAll()
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<SavedBook>>(json) ?? new List<SavedBook>();
    }
    public SavedBook GetById(string id)
    {
        return GetAll().FirstOrDefault(x => x.Id == id);
    }
    public void Add(SavedBook book)
    {
        var books = GetAll();
        books.Add(book);
        File.WriteAllText(filePath, JsonSerializer.Serialize(books));
    }
    public void Update(string id, SavedBook updated)
    {
        var books = GetAll();
        var book = books.FirstOrDefault(x => x.Id == id);

        if (book != null)
        {
            book.Title = updated.Title;
            book.Authors = updated.Authors;

            File.WriteAllText(filePath, JsonSerializer.Serialize(books));
        }
    }
    public void Delete(string id)
    {
        var books = GetAll();
        var book = books.FirstOrDefault(x => x.Id == id);

        if (book != null)
        {
            books.Remove(book);
            File.WriteAllText(filePath, JsonSerializer.Serialize(books));
        }
    }
}