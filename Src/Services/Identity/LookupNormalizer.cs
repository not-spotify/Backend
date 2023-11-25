using Microsoft.AspNetCore.Identity;

namespace MusicPlayerBackend.Services.Identity;

public sealed class LookupNormalizer : ILookupNormalizer
{
    public string? NormalizeName(string? name)
    {
        return name?.ToUpperInvariant();
    }

    public string? NormalizeEmail(string? email)
    {
        return email?.ToUpperInvariant();
    }
}
