namespace BuildingBlocks.Pagination;

public class PaginatedResult<TEntity>
{
    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public long Count { get; private set; }
    public IEnumerable<TEntity> Data { get; private set; }
    
    public PaginatedResult(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Data = data;
    }
}