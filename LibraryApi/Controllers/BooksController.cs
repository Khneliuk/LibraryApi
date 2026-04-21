using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;

namespace LibraryApi.Controllers;
[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private GoogleBooksService google = new();
    private BooksStorageService storage = new();
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var result = await google.SearchBooksAsync(query);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var book = await google.GetBookByIdAsync(id);

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