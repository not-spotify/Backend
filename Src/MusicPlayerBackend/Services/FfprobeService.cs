using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Options;

namespace MusicPlayerBackend.Services;

public sealed class FfprobeMetadata
{
    public Dictionary<string, string> Tags { get; set; } = null!;
    public string? Isrc { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string Extension { get; set; } = null!;
    public string RawJson { get; set; } = null!;

    public CoverInfo? CoverInfo { get; set; }
}

public sealed class CoverInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string CodecName { get; set; } = null!;
    public string PixFmt { get; set; } = null!;
    public Dictionary<string, int> Disposition { get; set; } = null!;
}

public sealed class FfprobeService(ILogger<FfprobeService> logger, IOptions<JsonSerializerOptions> jsonOptions)
{
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value;

    // TODO: Check ffmpeg tooling on host system

    public async Task<MemoryStream?> ExtractCover(MemoryStream ms, FfprobeMetadata meta)
    {
        if (meta.CoverInfo == null)
        {
            logger.LogDebug("CoverInfo empty. Won't continue.");
            return null;
        }

        var coverMemoryStream = new MemoryStream();
        coverMemoryStream.Position = 0;

        var errSb = new StringBuilder();
        ms.Position = 0;

        // ffmpeg can stuck
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var command = Cli.Wrap("ffmpeg")
            .WithArguments([
                "-v", "error", // Disable for debugging
                "-i", "pipe:0",
                "-an",
                "-f", meta.CoverInfo.CodecName,
                "-pix_fmt", meta.CoverInfo.PixFmt,
                "-s", meta.CoverInfo.Width + "x" + meta.CoverInfo.Width,
                "pipe:1"
            ], escape: false)
            .WithStandardInputPipe(PipeSource.FromStream(ms))
            .WithStandardOutputPipe(PipeTarget.ToStream(coverMemoryStream))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errSb))
            .WithValidation(CommandResultValidation.None);
        try
        {
            var commandResult = await command.ExecuteAsync(cts.Token);

            if (errSb.Length != 0)
            {
                await coverMemoryStream.DisposeAsync();
                logger.LogError("Got error: {error}", errSb.ToString());
                return null;
            }

            if (!commandResult.IsSuccess)
            {
                await coverMemoryStream.DisposeAsync();
                logger.LogError("Got status-code {code}", commandResult.ExitCode);
                return null;
            }

            return coverMemoryStream;
        }
        catch (Exception e)
        {
            logger.LogError("Error: {e}", e.Message);
            return null;
        }
    }

    public async Task<FfprobeMetadata?> FfprobeAnalyze(MemoryStream ms)
    {
        var errSb = new StringBuilder();
        var outSb = new StringBuilder();

        var commandResult = await Cli.Wrap("ffprobe")
            .WithArguments("-v quiet -print_format json -show_format -show_streams -sexagesimal -")
            .WithStandardInputPipe(PipeSource.FromStream(ms))
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outSb))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errSb))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        if (!commandResult.IsSuccess)
        {
            logger.LogError("Got error. Status-code: {sc}. Error: {err}", commandResult.ExitCode, errSb.ToString());
            return null;
        }

        var json = outSb.ToString();
        var res = JsonNode.Parse(json);

        var extension = res?["format"]?["format_name"]?.ToString();

        if (extension == null)
        {
            logger.LogDebug("Extension is empty. Leaving");
            return null;
        }

        if (extension.Contains(','))
        {
            logger.LogDebug("format_name detected as array. trying to extract audio format");
            if (extension.Contains("m4a"))
            {
                extension = "m4a";
                logger.LogDebug("extracted {ext}", extension);
            }
        }

        var tags = res?["format"]?["tags"];

        var tagsDict = tags?.Deserialize<Dictionary<string, string>>();
        if (tagsDict == null)
        {
            logger.LogDebug("Can't parse format tags. Leaving");
            return null;
        }

        var jIsrc = res?["format"]?["ISRC"] ?? res?["format"]?["isrc"];
        var jTitle = res?["format"]?["TITLE"] ?? res?["format"]?["title"];
        var jArtist = res?["format"]?["ARTIST"] ?? res?["format"]?["artist"];
        var streams = res?["streams"]?.AsArray();

        var coverInfo = new CoverInfo();

        if (streams?.Count > 0)
        {
            JsonNode? coverStream = null; // media content streams, not MemoryStream etc
            JsonNode? disposition = null;

            foreach (var stream in streams)
            {
                disposition = stream?["disposition"];
                if (stream?["disposition"]?["attached_pic"] == null
                    || disposition == null
                    || (disposition["attached_pic"]?.ToString() != "1" && stream["codec_type"]?.ToString() != "video"))
                    continue;

                coverStream = stream;
                break;
            }

            var coverInfoDisposition = disposition?.Deserialize<Dictionary<string, int>>();
            var codecName = coverStream?["codec_name"]?.ToString();
            var pixFmt = coverStream?["pix_fmt"]?.ToString();
            var rawWidth = coverStream?["width"]?.ToString();
            var rawHeight = coverStream?["height"]?.ToString();

            if (coverStream != null
                && coverInfoDisposition != null
                && codecName != null
                && pixFmt != null
                && int.TryParse(rawWidth, out var width)
                && int.TryParse(rawHeight, out var height))
            {
                coverInfo.Width = width;
                coverInfo.Height = height;
                coverInfo.CodecName = codecName;
                coverInfo.PixFmt = pixFmt;
                coverInfo.Disposition = coverInfoDisposition;
            }
            else
                coverInfo = null;
        }
        else
            logger.LogWarning("Can't streams. Cover won't be created");

        return new FfprobeMetadata
        {
            Tags = tagsDict,
            Isrc = jIsrc?.ToString(),
            Title = jTitle?.ToString(),
            Artist = jArtist?.ToString(),
            Extension = extension,
            RawJson = JsonSerializer.Serialize(res, _jsonOptions),
            CoverInfo = coverInfo
        };
    }
}
