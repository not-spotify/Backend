using Microsoft.AspNetCore.Identity;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Services.Identity;

public sealed class PasswordHasher : PasswordHasher<User>;
