namespace MusicPlayerBackend.Persistence.Repositories

open System.Linq
open System.Runtime.CompilerServices
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Persistence

[<Extension>]
type EntityFrameworkQueryableExtensions() =
    [<Extension>]
    static member TrySingle(queryable: IQueryable<'T>, ct) = task {
        let! result =
            match ct with
            | None ->
                queryable.SingleOrDefaultAsync()
            | Some ct ->
                queryable.SingleOrDefaultAsync(ct)

        return result |> Option.ofUncheckedObj
    }

    [<Extension>]
    static member TryFirst(queryable: IQueryable<'T>, ct) = task {
        let! result =
            match ct with
            | None ->
                queryable.FirstOrDefaultAsync()
            | Some ct ->
                queryable.FirstOrDefaultAsync(ct)

        return result |> Option.ofUncheckedObj
    }

    [<Extension>]
    static member Any(queryable: IQueryable<'T>, ct) = task {
        return!
            match ct with
            | None ->
                queryable.AnyAsync()
            | Some ct ->
                queryable.AnyAsync(ct)
    }

    [<Extension>]
    static member Count(queryable: IQueryable<'T>, ct) = task {
        return!
            match ct with
            | None ->
                queryable.CountAsync()
            | Some ct ->
                queryable.CountAsync(ct)
    }

    [<Extension>]
    static member ToArray(queryable: IQueryable<'T>, ct) = task {
        return!
            match ct with
            | None ->
                queryable.ToArrayAsync()
            | Some ct ->
                queryable.ToArrayAsync(ct)
    }
