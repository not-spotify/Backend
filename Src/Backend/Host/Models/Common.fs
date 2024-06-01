namespace MusicPlayerBackend.Host.Models

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type InvalidRequestResponse = {
    [<Required>]
    Message: string
}

type Entity<'TKey, 'T when 'T:(member Id: 'TKey)> = 'T

[<CLIMutable>]
type ListResponse<'TKey, 'T when Entity<'TKey, 'T>> = {
    Items: 'T[]
    Next: 'TKey
}
