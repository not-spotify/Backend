namespace MusicPlayerBackend.Contracts.User

type CreateUserRequest = {
    UserName: string
    Email: string

    Password: string
    ConfirmPassword: string
}

type UserId = System.Guid

type User = {
    Id: UserId
}
