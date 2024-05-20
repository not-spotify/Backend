namespace Contracts.Shared

type Id = System.Guid

[<RequireQualifiedAccess>]
module Id =
    let inline create () : Id = System.Guid.NewGuid()
