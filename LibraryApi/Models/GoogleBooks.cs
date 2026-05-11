namespace LibraryApi.Models;

public class GoogleBooks
{
    public string ExternalId { get; set; }  

    public string Title { get; set; }

    public List<string> Authors { get; set; } = new();

    public string Description { get; set; }
}