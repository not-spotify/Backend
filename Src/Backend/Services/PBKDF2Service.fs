namespace Services

open MusicPlayerBackend.Common

open System
open System.Security.Cryptography
open FSharp.NativeInterop

[<Struct>]
type PasswordVerificationResult =
    | Success
    | NeedRehash
    | Failed

type PasswordHasherVersion = {
    Algorithm: HashAlgorithmName
    SaltSize: int
    KeySize: int
    Iterations: int
}

#nowarn "9"

type PBKDF2PasswordHasher() =
    let [<Literal>] DefaultVersion = 0x02uy

    let versions = dict [
        (0x01uy, { Algorithm = HashAlgorithmName.SHA256; SaltSize = 256 / 8; KeySize = 256 / 8; Iterations = 600000 })
        (0x02uy, { Algorithm = HashAlgorithmName.SHA512; SaltSize = 512 / 8; KeySize = 512 / 8; Iterations = 210000 })
    ]

    member _.HashPassword (password: string) =
        if password = null || password.Length = 0 then
            ValueNone
        else
            let version = versions[DefaultVersion]
            let hashedPasswordByteCount = 1 + version.SaltSize + version.KeySize
            let hashedPasswordBytesPtr = NativePtr.stackalloc<byte> hashedPasswordByteCount |> NativePtr.toVoidPtr
            let hashedPasswordBytes = Span<byte>(hashedPasswordBytesPtr, hashedPasswordByteCount)

            let saltBytes = hashedPasswordBytes.Slice(start = 1, length = version.SaltSize);
            let keyBytes = hashedPasswordBytes.Slice(start = 1 + version.SaltSize, length = version.KeySize);
            hashedPasswordBytes[0] <- DefaultVersion;
            RandomNumberGenerator.Fill(saltBytes);
            Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, keyBytes, version.Iterations, version.Algorithm);

            ValueSome ^ Convert.ToBase64String(hashedPasswordBytes)

    member _.VerifyHashedPassword (hashedPassword: string, providedPassword: string) =
        let version = versions[DefaultVersion]
        let hashedPasswordByteCount = 1 + version.SaltSize + version.KeySize
        let hashedPasswordBytesPtr = NativePtr.stackalloc<byte> hashedPasswordByteCount |> NativePtr.toVoidPtr
        let hashedPasswordBytes = Span<byte>(hashedPasswordBytesPtr, hashedPasswordByteCount)

        let guard = Convert.TryFromBase64String(hashedPassword, hashedPasswordBytes) |> ValueOption.ofTry |> ValueOption.isSome
        if not guard then
            PasswordVerificationResult.Failed
        elif hashedPasswordBytes.Length = 0 then
            PasswordVerificationResult.Failed
        else
            let versionId = hashedPasswordBytes[0]
            let version = versions[versionId]

            let expectedHashedPasswordLength = 1 + version.SaltSize + version.KeySize
            if hashedPasswordBytes.Length <> expectedHashedPasswordLength then
                PasswordVerificationResult.Failed
            else
                let saltBytes = hashedPasswordBytes.Slice(start = 1, length = version.SaltSize)
                let expectedKeyBytes = hashedPasswordBytes.Slice(start = 1 + version.SaltSize, length = version.KeySize)
                let actualKeyBytesPtr = NativePtr.stackalloc<byte> version.KeySize |> NativePtr.toVoidPtr
                let actualKeyBytes = Span<byte>(actualKeyBytesPtr, version.KeySize)

                Rfc2898DeriveBytes.Pbkdf2(providedPassword, saltBytes, actualKeyBytes, version.Iterations, version.Algorithm)

                if CryptographicOperations.FixedTimeEquals(expectedKeyBytes, actualKeyBytes) |> not then
                    PasswordVerificationResult.Failed
                elif versionId <> DefaultVersion then
                    PasswordVerificationResult.NeedRehash
                else
                    PasswordVerificationResult.Success
