using LibraryApi.Model.Library;
using LibraryApi.Storage;

namespace LibraryApi.Services;

public class LibraryService
{
    private readonly JsonStorage storage;

    public LibraryService(JsonStorage storage)
    {
        this.storage = storage;
    }

    public async Task<List<LibraryEntry>> GetAllAsync()
    {
        return await this.storage.ReadAllAsync();
    }

    public async Task<LibraryEntry> GetByIdAsync(int id)
    {
        var all = await this.storage.ReadAllAsync();

        foreach (var entry in all)
        {
            if (entry.Id == id)
            {
                return entry;
            }
        }

        return null;
    }

    public async Task<LibraryEntry> CreateAsync(CreateLibraryEntryDto dto)
    {
        var all = await this.storage.ReadAllAsync();

        int newId;
        if (all.Count == 0)
        {
            newId = 1;
        }
        else
        {
            newId = all.Max(e => e.Id) + 1;
        }

        var entry = new LibraryEntry();
        entry.Id = newId;
        entry.GoogleBooksId = dto.GoogleBooksId;
        entry.Title = dto.Title;
        entry.Authors = dto.Authors;
        entry.Categories = dto.Categories;
        entry.Status = dto.Status;
        entry.Notes = "";
        entry.AddedAt = DateTime.UtcNow;

        all.Add(entry);
        await this.storage.WriteAllAsync(all);

        return entry;
    }

    public async Task<LibraryEntry> UpdateAsync(int id, UpdateLibraryEntryDto dto)
    {
        var all = await this.storage.ReadAllAsync();

        LibraryEntry entry = null;
        foreach (var e in all)
        {
            if (e.Id == id)
            {
                entry = e;
                break;
            }
        }

        if (entry == null)
        {
            return null;
        }

        if (dto.Status != null)
        {
            entry.Status = dto.Status;
        }

        if (dto.PersonalRating != 0)
        {
            entry.PersonalRating = dto.PersonalRating;
        }

        if (dto.Notes != null)
        {
            entry.Notes = dto.Notes;
        }

        entry.StartedAt = dto.StartedAt;
        entry.FinishedAt = dto.FinishedAt;

        await this.storage.WriteAllAsync(all);

        return entry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var all = await this.storage.ReadAllAsync();

        LibraryEntry entry = null;
        foreach (var e in all)
        {
            if (e.Id == id)
            {
                entry = e;
                break;
            }
        }

        if (entry == null)
        {
            return false;
        }

        all.Remove(entry);
        await this.storage.WriteAllAsync(all);

        return true;
    }
}