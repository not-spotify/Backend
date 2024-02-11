namespace MusicPlayerBackend.Host.Controllers

open System
open System.Net.Mime
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Common.TypeExtensions
open MusicPlayerBackend.Host
open MusicPlayerBackend.Host.Services
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.TransferObjects
open MusicPlayerBackend.TransferObjects.User

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<Route("[controller]/[action]")>]
type UserController(
        userManager: UserManager<User>,
        signInManager: SignInManager<User>,
        userProvider: UserProvider,
        refreshTokenRepository: FsharpRefreshTokenRepository,
        playlistRepository: FsharpPlaylistRepository,
        userRepository: FsharpUserRepository,
        unitOfWork: FsharpUnitOfWork,
        jwtService: JwtService) =

    inherit ControllerBase()

    /// <summary>
    ///     Gets authorized User.
    /// </summary>
    /// <returns>Authorized User</returns>
    /// <response code="200">Returns User</response>
    /// <response code="400">Wrong schema</response>
    /// <response code="401">Not authorized</response>
    [<Authorize>]
    [<HttpGet(Name = "GetMe")>]
    [<ProducesResponseType(typeof<UserResponse>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
    member this.GetMe() = task {
        let! user = userProvider.GetUser()

        return this.Ok ^ UserResponse(
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        )
    }

    /// <summary>
    ///     Creates User.
    /// </summary>
    /// <returns>User identifier and JWT Bearer</returns>
    /// <response code="200">Returns User identifier</response>
    /// <response code="400">Wrong schema</response>
    [<HttpPost(Name = "RegisterUser")>]
    [<ProducesResponseType(typeof<RegisterResponse>, StatusCodes.Status200OK)>]
    member this.Register(request: RegisterRequest) = task {
        let user = User.Create(request.UserName, request.Email, request.Password, Guid.Empty)
        let! result = userManager.CreateAsync(user = user, password = request.Password)
        match result.Succeeded with
        | true ->
            let! _ = unitOfWork.BeginTransaction()
            let trackedUser = userRepository.Save(user)
            do! unitOfWork.SaveChanges()

            let user = trackedUser.Entity

            let playlist = Playlist.Create($"{request.UserName}'s Favorites", Visibility.Private, None, user.Id)

            let trackedPlaylist = playlistRepository.Save(playlist)
            do! unitOfWork.SaveChanges()
            let playlist = trackedPlaylist.Entity

            user.FavoritePlaylistId <- playlist.Id
            do! unitOfWork.SaveChanges()
            do! unitOfWork.Commit()

            return this.Ok ^ RegisterResponse(Id = user.Id) :> IActionResult
        | false ->
            return this.BadRequest(result) :> IActionResult
    }

    /// <summary>
    /// Gets JWT Bearer for using secure actions.
    /// </summary>
    /// <returns>JWT Bearer</returns>
    /// <response code="200">Returns JWT Bearer</response>
    /// <response code="401">Wrong email or password</response>
    [<HttpPost(Name = "LogInUser")>]
    [<ProducesResponseType(typeof<LoginResponse>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(typeof<LoginResponse>, StatusCodes.Status401Unauthorized)>]
    member this.Login(request: LoginRequest) = task {
        let! user = userManager.FindByEmailAsync(request.Email)
        let user = // TODO: Implement User for domain level
            if Object.ReferenceEquals(user, null) then
                None
            else
                Some user

        match user with
        | None ->
            return this.Unauthorized(UnauthorizedResponse(Error = "Can't find user or wrong password")) :> IActionResult
        | Some user ->
            let! signInResult = signInManager.CheckPasswordSignInAsync(user, request.Password, false)
            if not signInResult.Succeeded then
                return this.Unauthorized(UnauthorizedResponse(Error = string signInResult)) :> IActionResult
            else
                let! jwtResponse = jwtService.Generate(user.Id)

                return this.Ok ^ LoginResponse(
                    JwtBearer = jwtResponse.JwtBearer,
                    RefreshToken = jwtResponse.RefreshToken,
                    RefreshTokenValidDue = jwtResponse.RefreshTokenValidDue,
                    JwtBearerValidDue = jwtResponse.JwtBearerValidDue,
                    UserId = user.Id) :> IActionResult
        }

    /// <summary>
    ///     Gets new JWT Bearer by RefreshToken.
    /// </summary>
    [<HttpPost(Name = "RefreshUserToken")>]
    [<ProducesResponseType(typeof<LoginResponse>, StatusCodes.Status200OK)>]
    [<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
    member this.Refresh(request: RefreshRequest) = task {
        let! existingRefreshToken = refreshTokenRepository.TryGetValid(request.UserId, request.Jti, request.RefreshToken)
        match existingRefreshToken with
        | None ->
            return this.Unauthorized(UnauthorizedResponse(Error = "Can't refresh Jwt Bearer")) :> IActionResult
        | Some existingRefreshToken ->
            let! _ = unitOfWork.BeginTransaction()
            existingRefreshToken.Revoked <- true
            %refreshTokenRepository.Save(existingRefreshToken)

            let! jwtResponse = jwtService.Generate(existingRefreshToken.UserId)
            do! unitOfWork.Commit()

            return this.Ok ^ LoginResponse(
                JwtBearer = jwtResponse.JwtBearer,
                RefreshToken = jwtResponse.RefreshToken,
                RefreshTokenValidDue = jwtResponse.RefreshTokenValidDue,
                JwtBearerValidDue = jwtResponse.JwtBearerValidDue,
                UserId = existingRefreshToken.UserId) :> IActionResult
        }
