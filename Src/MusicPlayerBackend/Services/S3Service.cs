﻿using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel.Args;

namespace MusicPlayerBackend.Services;

public interface IS3Service
{
    ValueTask<string?> TryGetFileUri(string bucket, string key);
    ValueTask<string?> TryUploadFileStream(string bucket, string key, Stream stream, string extension, CancellationToken ct = default);
}

public sealed class S3Service(ILogger<S3Service> logger, IMinioClient minioClient) : IS3Service
{
    public async ValueTask<string?> TryGetFileUri(string bucket, string key)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(key)
            .WithExpiry(1200);

        return await minioClient.PresignedGetObjectAsync(args);
    }

    public async ValueTask<string?> TryUploadFileStream(string bucket, string key, Stream stream, string extension, CancellationToken ct = default)
    {
        try
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;
            var fileName = key + extension;

            var args = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithContentType(new FileExtensionContentTypeProvider().Mappings[extension])
                .WithObjectSize(ms.Length)
                .WithStreamData(ms);

            await minioClient.PutObjectAsync(args, ct);
            return await TryGetFileUri(bucket, key);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to upload file stream");
            return default;
        }
    }
}
