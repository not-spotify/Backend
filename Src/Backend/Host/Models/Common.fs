namespace MusicPlayerBackend.Host.Models

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type InvalidRequestResponse = {
    [<Required>]
    Message: string
}
