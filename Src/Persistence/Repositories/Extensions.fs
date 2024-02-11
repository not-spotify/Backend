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
