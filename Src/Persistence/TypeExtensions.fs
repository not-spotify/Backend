[<AutoOpen>]
module MusicPlayerBackend.Persistence.TypeExtensions

module Option =
    let inline ofUncheckedObj obj =
        if System.Object.ReferenceEquals(null, obj) then
            None
        else
            Some obj

    let inline toUncheckedObj obj =
        if Option.isNone obj then
            Unchecked.defaultof<_>
        else
            Option.get obj

module TaskOption =
    let inline ofUncheckedObj obj = task {
        let! obj = obj
        if System.Object.ReferenceEquals(null, obj) then
            return None
        else
            return Some obj
    }

    let inline toUncheckedObj obj = task {
        let! obj = obj
        if Option.isNone obj then
            return Unchecked.defaultof<_>
        else
            return Option.get obj
    }
