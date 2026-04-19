using LibraryApi.Model.GoogleBooks;
namespace LibraryApi.Model;

public class Book
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Authors { get; set; }
    public string Publisher { get; set; }
    public string PublishedDate { get; set; }
    public string Description { get; set; }
    public string Categories { get; set; }
    public double Rating { get; set; }
    public int PageCount { get; set; }
    public string Language { get; set; }
    public static Book FromVolume(VolumeItem item)
    {
        var v = item.VolumeInfo;
        var book = new Book();
        book.Id = item.Id;

        if (v == null)
        {
            return book;
        }

        book.Title = v.Title;
        book.Publisher = v.Publisher;
        book.PublishedDate = v.PublishedDate;
        book.Description = v.Description;
        book.Rating = v.AverageRating;
        book.PageCount = v.PageCount;
        book.Language = v.Language;

        if (v.Authors != null)
        {
            book.Authors = string.Join(", ", v.Authors);
        }

        if (v.Categories != null)
        {
            book.Categories = string.Join(", ", v.Categories);
        }
        return book;
    }
}