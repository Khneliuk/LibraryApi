using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly GoogleBooksService googleBooksService;

    public BooksController(GoogleBooksService googleBooksService)
    {
        this.googleBooksService = googleBooksService;
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Параметр query є обов'язковим" });
        }

        if (maxResults < 1 || maxResults > 40)
        {
            return BadRequest(new { error = "maxResults має бути від 1 до 40" });
        }

        var results = await this.googleBooksService.SearchBooksAsync(query, maxResults);
        return Ok(new { total = results.Count, items = results });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { error = "ID є обов'язковим" });
        }

        var book = await this.googleBooksService.GetBookByIdAsync(id);

        if (book == null)
        {
            return NotFound(new { error = $"Книгу з ID '{id}' не знайдено" });
        }

        return Ok(book);
    }
}