namespace SharedKernel.DTOs;

public class PagedList<T>
{
    public List<T> Items { get; set; }

    public int TotalPages { get; set; }

    public long TotalItems { get; set; }

    public PagedList(List<T> items, long totalItems,int pageSize)
    {
         Items = items;
         TotalItems = totalItems;
         TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }
    
    public PagedList()
    {
        
    }
}