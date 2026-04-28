using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;

namespace LibraryApi.Controllers;
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly GoogleBooksService google;
    private readonly BooksStorageService storage;
    public BooksController(GoogleBooksService google, BooksStorageService storage)
    {
        this.google = google;
        this.storage = storage;
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var result = await google.SearchBooksAsync(query);
        return Ok(result);
    }
    [HttpGet]
    public IActionResult GetAll()
    {
        var books = storage.GetAll();
        return Ok(books);
    }
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var book = storage.GetById(id);

        if (book == null)
            return NotFound();

        return Ok(book);
    }
    [HttpPost]
    public IActionResult Create(SavedBook book)
    {
        storage.Add(book);
        return StatusCode(201);
    }
    [HttpPut("{id}")]
    public IActionResult Update(string id, SavedBook book)
    {
        var existing = storage.GetById(id);

        if (existing == null)
            return NotFound();

        storage.Update(id, book);
        return Ok();
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var existing = storage.GetById(id);

        if (existing == null)
            return NotFound();

        storage.Delete(id);
        return NoContent();
    }
}