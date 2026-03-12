namespace Shared.Contracts.Models;

public class PagedResponse<T> : BaseResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    public PagedResponse(T data, int pageNumber, int pageSize, int totalRecords) 
        : base(data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }
}