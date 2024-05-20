namespace MusicPlayerBackend.Host.Ext

open System.Runtime.CompilerServices
open Minio
open Minio.DataModel.Args

type MinioExtensions() =

    [<Extension>]
    static member BucketExists(mc: IMinioClient, args: BucketExistsArgs) =
        mc.BucketExistsAsync(args).ConfigureAwait(false).GetAwaiter().GetResult()

    [<Extension>]
    static member MakeBucket(mc: IMinioClient, args: MakeBucketArgs) =
        mc.MakeBucketAsync(args).ConfigureAwait(false).GetAwaiter().GetResult()
        mc

    [<Extension>]
    static member CreateBucketIfNotExists(mc: IMinioClient, name) =
        if mc.BucketExists(BucketExistsArgs().WithBucket(name)) = false then
            mc.MakeBucket(MakeBucketArgs().WithBucket(name))
        else
            mc
