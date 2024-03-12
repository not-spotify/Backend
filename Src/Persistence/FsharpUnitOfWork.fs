namespace MusicPlayerBackend.Persistence

open System.Threading.Tasks
open Microsoft.EntityFrameworkCore.Storage

type FsharpUnitOfWork(ctx: FsharpAppDbContext) =
    let mutable t : IDbContextTransaction = null

    member _.SaveChanges(?ct) =
        match ct with
        | None -> ctx.SaveChangesAsync() :> Task
        | Some ct -> ctx.SaveChangesAsync(ct) :> Task

    member _.BeginTransaction(?ct) = task {
        let! transaction =
            match ct with
            | None -> ctx.Database.BeginTransactionAsync()
            | Some ct -> ctx.Database.BeginTransactionAsync(ct)
        t <- transaction
    }

    member _.Commit(?ct) =
        match ct with
        | None -> t.CommitAsync()
        | Some ct -> t.CommitAsync(ct)
