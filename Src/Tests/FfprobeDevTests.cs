using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicPlayerBackend.Services;
using NUnit.Framework;

namespace Tests;

[Category("Not tests")]
public class FfprobeDevTests
{
    private IOptions<JsonSerializerOptions> _jsonSerializer = null!;
    private ILoggerFactory _loggerFactory = null!;

    [SetUp]
    public void Setup()
    {
        _jsonSerializer = Options.Create(new JsonSerializerOptions());
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }

    [Test]
    [Explicit] // For debugging
    public async Task Covers()
    {
        // ReSharper disable once StringLiteralTypo
        const string musicPath = "/run/media/td/2TB/μζeq/with covers/";
        const string outPath = "/home/td/covers/";

        // var files = Directory.GetFiles(musicPath);
        // string[] files = ["/run/media/td/2TB/μζeq/with covers/palomatic trill -1995- -album-.mp4"];
        string[] files = [ Path.Combine(musicPath, "Clark - Growls Garden.flac") ];
        foreach (var file in files)
        {
            var ffprobeService = new FfprobeService(_loggerFactory.CreateLogger<FfprobeService>(), _jsonSerializer);
            var trackMs = new MemoryStream();

            await File.OpenRead(file).CopyToAsync(trackMs);
            trackMs.Position = 0;

            var ffprobeMetadata = await ffprobeService.FfprobeAnalyze(trackMs);

            if (ffprobeMetadata?.CoverInfo == null)
                Assert.Fail("Where's cover");

            trackMs.Position = 0;
            var coverMemoryStream = await ffprobeService.ExtractCover(trackMs, ffprobeMetadata!);

            if (coverMemoryStream == null)
            {
                // Assert.Fail("Cover wasn't extracted");
                continue;
            }

            coverMemoryStream.Position = 0;

            var name = ffprobeMetadata!.Title ?? Guid.NewGuid().ToString();
            var fileName = $"{name}.{ffprobeMetadata!.CoverInfo!.CodecName}";

            Directory.GetFiles(outPath)
                .ToList()
                .ForEach(File.Delete);

            var coverFullName = Path.Combine(outPath, fileName);
            await File.WriteAllBytesAsync(coverFullName, coverMemoryStream.ToArray());
            await coverMemoryStream.DisposeAsync();

            Assert.That(Directory.GetFiles(outPath), Is.Not.Empty);
        }
    }
}
