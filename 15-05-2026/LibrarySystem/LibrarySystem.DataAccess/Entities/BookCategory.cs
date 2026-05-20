using System;


namespace LibrarySystem.DataAccess.Entities
{
    public class BookCategory{

    public int Id {get; set;}
    public string CategoryName {get;set;} = string.Empty;
    public bool IsActive {get; set;} = true;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public virtual ICollection<Book> Books {get; set;} = new List<Book>();
    }

}