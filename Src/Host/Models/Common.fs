module MusicPlayerBackend.Host.Models.Common

type UnauthorizedResponse = {
    Error: string
}

type ItemsResponse<'T> = {
    Count: int
    Items: 'T
}
