namespace MusicPlayerBackend.Host.Models

type BadResponse = {
    Error: string
}

type UnauthorizedResponse = BadResponse

type ItemsResponse<'T> = {
    PageNumber: int
    TotalCount: int
    Items: 'T[]
}
