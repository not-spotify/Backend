namespace MusicPlayerBackend.Host.Services

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.JsonWebTokens
open Microsoft.IdentityModel.Tokens

open MusicPlayerBackend.Common
open MusicPlayerBackend.OptionSections
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities

type JwtBearerResponse = {
    JwtBearer: string
    RefreshToken: string
    RefreshTokenValidDue: DateTime
    JwtBearerValidDue: DateTime
}

[<Sealed>]
type JwtService(
    tokenConfig: IOptions<TokenConfig>,
    refreshTokenRepository: FsharpRefreshTokenRepository,
    unitOfWork: FsharpUnitOfWork) =

    let tokenConfig = tokenConfig.Value

    member _.Generate(userId: Guid) = task {
        let jti = Guid.NewGuid()
        let refreshTokenValue = Guid.NewGuid()
        let jwtValidDue = DateTime.UtcNow.AddDays(1) // TODO: Move to appconfig
        let refreshValidDue = jwtValidDue.AddDays(7)

        let secKey = System.Text.Encoding.UTF8.GetBytes(tokenConfig.SigningKey)
        let signCred = SigningCredentials(SymmetricSecurityKey(secKey), SecurityAlgorithms.HmacSha256Signature)

        let tokenDescriptor = SecurityTokenDescriptor(
            Subject = ClaimsIdentity([|
                Claim(ClaimTypes.NameIdentifier, string userId)
                Claim(JwtRegisteredClaimNames.Jti, string jti)
            |]),
            Expires = jwtValidDue,
            SigningCredentials = signCred)

        let tokenHandler = JwtSecurityTokenHandler()
        let token = tokenHandler.CreateToken(tokenDescriptor)
        let refreshToken = RefreshToken.Create(userId, jwtValidDue, jti, refreshTokenValue)

        %refreshTokenRepository.Save(refreshToken)
        do! unitOfWork.SaveChanges()

        return {
            JwtBearer = tokenHandler.WriteToken(token)
            RefreshToken = string refreshTokenValue
            RefreshTokenValidDue = refreshValidDue
            JwtBearerValidDue = jwtValidDue
        }
    }
