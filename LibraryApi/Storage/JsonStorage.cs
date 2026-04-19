using System.Text.Json;
using LibraryApi.Model.Library;

namespace LibraryApi.Storage;

public class JsonStorage
{
    private readonly string filePath;

    public JsonStorage(IConfiguration config)
    {
        this.filePath = config["StoragePath"];

        var dir = Path.GetDirectoryName(this.filePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!File.Exists(this.filePath))
        {
            File.WriteAllText(this.filePath, "[]");
        }
    }

    public async Task<List<LibraryEntry>> ReadAllAsync()
    {
        var json = await File.ReadAllTextAsync(this.filePath);
        var entries = JsonSerializer.Deserialize<List<LibraryEntry>>(json);

        if (entries == null)
        {
            return new List<LibraryEntry>();
        }

        return entries;
    }

    public async Task WriteAllAsync(List<LibraryEntry> entries)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(entries, options);
        await File.WriteAllTextAsync(this.filePath, json);
    }
}