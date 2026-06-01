namespace LibrarySystem.DTOs
{
    public class BookFilterWithPaginationDTO : PaginationDTO
    {
        public int? FromPublicationYear {get; set;}
        public int? ToPublicationYear {get; set;}
        public string? Title {get; set;}

        public string? Author {get; set;}
    }
}