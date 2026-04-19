using LibraryApi.Model.Library;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService libraryService;

    public LibraryController(LibraryService libraryService)
    {
        this.libraryService = libraryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entries = await this.libraryService.GetAllAsync();
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entry = await this.libraryService.GetByIdAsync(id);

        if (entry == null)
        {
            return NotFound(new { error = $"Запис з ID '{id}' не знайдено" });
        }

        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLibraryEntryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { error = "Назва книги є обов'язковою" });
        }

        var validStatuses = new List<string> { "WantToRead", "Reading", "Read", "Dropped" };

        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new { error = "Статус має бути одним з: WantToRead, Reading, Read, Dropped" });
        }

        var entry = await this.libraryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLibraryEntryDto dto)
    {
        if (dto.PersonalRating < 0 || dto.PersonalRating > 5)
        {
            return BadRequest(new { error = "Рейтинг має бути від 1 до 5." });
        }

        var entry = await this.libraryService.UpdateAsync(id, dto);

        if (entry == null)
        {
            return NotFound(new { error = $"Запис з ID '{id}' не знайдено." });
        }

        return Ok(entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await this.libraryService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { error = $"Запис з ID '{id}' не знайдено." });
        }

        return NoContent();
    }
}