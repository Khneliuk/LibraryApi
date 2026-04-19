using System.Text.Json.Serialization;

namespace LibraryApi.Model.GoogleBooks;

public class VolumeInfo
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; }

    [JsonPropertyName("publisher")]
    public string Publisher { get; set; }

    [JsonPropertyName("publishedDate")]
    public string PublishedDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; }

    [JsonPropertyName("averageRating")]
    public double AverageRating { get; set; }

    [JsonPropertyName("ratingsCount")]
    public int RatingsCount { get; set; }

    [JsonPropertyName("pageCount")]
    public int PageCount { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }
}