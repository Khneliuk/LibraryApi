using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;

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

    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var books = await database.GetBooksAsync();

        var result = books
            .Where(b => b.Title != null &&
                        b.Title.ToLower().Contains(query.ToLower()))
            .ToList();

        return Ok(result);
    }

    [HttpGet("author")]
    public async Task<IActionResult> SearchByAuthor(string author)
    {
        var result = await google.SearchByAuthorAsync(author);
        return Ok(result);
    }

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

    [HttpPost]
    public async Task<IActionResult> Create(SavedBook book)
    {
        if (book.Rating < 0 || book.Rating > 10)
            return BadRequest("Rating must be 0–10");

        await database.AddBookAsync(book);
        return StatusCode(201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SavedBook book)
    {
        var existing = await database.GetByIdAsync(id);

        if (existing == null)
            return NotFound();

        book.Id = id;
        await database.UpdateBookAsync(book);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await database.GetByIdAsync(id);

        if (existing == null)
            return NotFound();

        await database.DeleteBookAsync(id);
        return NoContent();
    }
}