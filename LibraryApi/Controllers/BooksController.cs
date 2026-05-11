using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
using LibraryApi.Services;
namespace LibraryApi.Controllers;
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly GoogleBooksService google;
    private readonly DatabaseService database;

    public BooksController(GoogleBooksService google, DatabaseService database)
    {
        this.google = google;
        this.database = database;
    }

    //пошук в гугл букс (назва)
    [HttpGet("google")]
    public async Task<IActionResult> GoogleSearch(string query)
    {
        var result = await google.SearchBooksAsync(query);
        return Ok(result);
    }

    //пошук в гугл букс (автор)
    [HttpGet("google/author")]
    public async Task<IActionResult> GoogleByAuthor(string author)
    {
        var result = await google.SearchByAuthorAsync(author);
        return Ok(result);
    }

    //пошук у бібліотеці користувача
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var books = await database.GetBooksAsync();

        var q = query?.ToLower() ?? "";

        var result = books
            .Where(b =>
                (!string.IsNullOrEmpty(b.Title) &&
                 b.Title.ToLower().Contains(q))
                ||
                (b.Authors != null &&
                 b.Authors.Any(a =>
                     !string.IsNullOrEmpty(a) &&
                     a.ToLower().Contains(q)))
            )
            .ToList();

        return Ok(result);
    }

    //список книг
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await database.GetBooksAsync());
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await database.GetByIdAsync(id);

        if (book == null)
            return NotFound();

        return Ok(book);
    }

    //додавання вручну
    [HttpPost]
    public async Task<IActionResult> Create(SavedBook book)
    {
        await database.AddBookAsync(book);
        return Ok();
    }

    //додавання з гугл букс
    [HttpPost("google/add")]
    public async Task<IActionResult> AddFromGoogle(SavedBook book)
    {
        await database.AddBookAsync(book);
        return Ok();
    }

    //оновити статус і рейтинг
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SavedBook book)
    {
        var existing = await database.GetByIdAsync(id);
        if (existing == null) return NotFound();

        book.Id = id;
        await database.UpdateBookAsync(book);

        return Ok();
    }

    //видалити
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await database.GetByIdAsync(id);
        if (existing == null) return NotFound();

        await database.DeleteBookAsync(id);
        return NoContent();
    }
}