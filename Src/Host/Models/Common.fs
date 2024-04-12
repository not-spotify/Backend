namespace MusicPlayerBackend.Host.Models

[<CLIMutable>]
type BadResponse = {
    Error: string
}

type UnauthorizedResponse = BadResponse

[<CLIMutable>]
type ItemsResponse<'T> = {
    PageNumber: int
    TotalCount: int
    Items: 'T[]
}
