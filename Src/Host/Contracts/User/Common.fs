namespace MusicPlayerBackend.Host.Contracts.User

type CreateUserRequest = {
    UserName: string
    Email: string

    Password: string
    ConfirmPassword: string
}
