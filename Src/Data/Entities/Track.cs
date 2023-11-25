namespace MusicPlayerBackend.Data.Entities;

public sealed record Track : EntityBase
{
    public string? Cover { get; set; }
}

