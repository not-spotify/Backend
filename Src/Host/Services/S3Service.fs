namespace MusicPlayerBackend.Host.Services

open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.Logging
open Minio
open Minio.DataModel.Args

type S3Service(logger: ILogger<S3Service>, minioClient: IMinioClient) =
    member _.TryGetFileUri(bucket: string, key: string) = task {
        let args =
            PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithExpiry(1200);

        return! minioClient.PresignedGetObjectAsync(args)
    }

    member this.TryUploadFileStream(bucket: string, key: string, stream: Stream, extension: string) = task {
        try
            use ms = new MemoryStream()
            do! stream.CopyToAsync(ms)
            ms.Position <- 0
            let fileName = key + extension

            let args =
                PutObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName)
                    .WithContentType(FileExtensionContentTypeProvider().Mappings[extension])
                    .WithObjectSize(ms.Length)
                    .WithStreamData(ms);

            do! minioClient.PutObjectAsync(args) :> Task
            let! result = this.TryGetFileUri(bucket, key)
            return Some result
        with e ->
            logger.LogError(e, "Failed to upload file stream")
            return None
    }
