using System.Text.Json.Serialization;
namespace LibraryApi.Model.GoogleBooks;

public class BookSearchResponse
{
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("items")]
    public List<VolumeItem> Items { get; set; }
}

public class VolumeItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public VolumeInfo VolumeInfo { get; set; }
}