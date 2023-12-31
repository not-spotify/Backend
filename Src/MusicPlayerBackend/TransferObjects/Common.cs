﻿using System.ComponentModel.DataAnnotations;

namespace MusicPlayerBackend.TransferObjects;

public sealed class UnauthorizedResponse
{
    public string Error { get; set; } = null!;
}

public abstract class PaginationRequestBase
{
    public int Page { get; set; }

    [Range(5, 20)]
    public int PageSize { get; set; }
}
