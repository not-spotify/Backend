namespace MusicPlayerBackend.Host

open System

[<Sealed; AllowNullLiteral; AttributeUsage(AttributeTargets.Method)>]
type AllowAnonymousOnlyAttribute() =
    inherit Attribute()
