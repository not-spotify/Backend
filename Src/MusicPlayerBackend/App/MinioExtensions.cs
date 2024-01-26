using Minio;
using Minio.DataModel.Args;

namespace MusicPlayerBackend.App;

public static class MinioExtensions
{
    public static bool BucketExists(this IMinioClient minioClient, BucketExistsArgs args)
    {
        return minioClient.BucketExistsAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static IMinioClient MakeBucket(this IMinioClient minioClient, MakeBucketArgs args)
    {
        minioClient.MakeBucketAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

        return minioClient;
    }

    public static IMinioClient CreateBucketIfNotExists(this IMinioClient minioClient, string name)
    {
        if (!minioClient.BucketExists(new BucketExistsArgs().WithBucket(name)))
            minioClient.MakeBucket(new MakeBucketArgs().WithBucket(name));

        return minioClient;
    }
}
