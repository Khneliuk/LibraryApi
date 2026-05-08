namespace LibraryApi.Models;
public class SavedBook
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<string> Authors { get; set; }
    public string Status { get; set; }
    public int Rating { get; set; }
}