namespace LibraryApi.Model.Library;

public class LibraryEntry
{
    public int Id { get; set; }
    public string GoogleBooksId { get; set; }
    public string Title { get; set; }
    public string Authors { get; set; }
    public string Categories { get; set; }
    public string Status { get; set; }
    public int PersonalRating { get; set; }
    public string Notes { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}