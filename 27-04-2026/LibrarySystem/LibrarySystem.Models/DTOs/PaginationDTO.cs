namespace LibrarySystem.DTOs
{
    public class PaginationDTO
    {
        public int PageNumber {get; set;} = 1;
        public int PageSize {get; set;} = 10;
    }

    public class PaginatedResponseDTO<T>
    {
        public List<T> Items {get; set;} = new List<T>();
        public int TotalCount {get; set;}
        public int PageNumber {get; set;}
        public int PageSize {get; set;}
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling((double)TotalCount / PageSize);
            }
        } 
        public bool HasPreviousPage 
        {
            get {
                return PageNumber > 1;
            }
        }
        public bool HasNextPage
        {
            get
            {
                return PageNumber < TotalPages;
            }
        }

    }
}
