using System.ComponentModel.DataAnnotations;

namespace MusicPlayerBackend.TransferObjects;

public sealed class UnauthorizedResponse
{
    public string Error { get; set; } = null!;
}

public abstract class PaginationRequestBase
{
    public int Page { get; set; } = 0;

    [Range(1, 200)]
    public int PageSize { get; set; } = 10;
}

public abstract class ItemsResponseAbstract<T>
{
    public int Count { get; set; }
    public T[] Items { get; set; } = null!;
}
