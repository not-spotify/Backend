module Domain.Tests.CreateEnumByName

open System
open Domain.Shared
open Domain.User

open NUnit.Framework

[<Test>]
let ``createEnumByName works properly`` () =
    let actual =
        EmailError.TooShort(2, 1)
        |> Reflection.createEnumByName<UserFieldValidationFailed, EmailError> "Email"

    match actual with
    | Ok (UserFieldValidationFailed.Email (EmailError.TooShort (2, 1))) ->
        Assert.Pass()
    | _ ->
        Assert.Fail(string actual)

[<Test>]
let ``createEnumByName handles non-existing field`` () =
    let actual =
        EmailError.TooShort(2, 1)
        |> Reflection.createEnumByName<UserFieldValidationFailed, EmailError> "SomeNonExistingField"

    match actual with
    | Error Reflection.ReflectionError.FieldNotFound ->
        Assert.Pass()
    | _ ->
        Assert.Fail(string actual)

[<Test>]
let ``createEnumByName handles invalid types`` () =
    let actual =
        555
        |> Reflection.createEnumByName<UserFieldValidationFailed, int> "Email"

    match actual with
    | Error Reflection.ReflectionError.NotUnionType ->
        Assert.Pass()
    | _ ->
        Assert.Fail(string actual)

#nowarn "0077"
#nowarn "0042"


module Core =
    /// unsafe cast like in C#
    let inline ucast<'a, 'b> (a: 'a): 'b = (# "" a: 'b #)
