namespace Domain.Shared

open System.Threading.Tasks
open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type FieldName = private FieldName of string

module FieldName =
    let create (p: System.Reflection.PropertyInfo) =
        FieldName p.Name

    let value (FieldName v) = v

module Reflection =
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Reflection

    let [<Literal>] private InvalidTypeMessage = "Unexpected Union type parameter"

    let private eval = Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.EvaluateQuotation

    [<RequireQualifiedAccess>]
    type ReflectionError =
        | NotUnionType
        | FieldNotFound

    let getFieldNameAndValue<'TField> (exp: Expr<'TField>) : (FieldName * 'TField) option =
        match exp, eval exp |> tryUcast with
        | Patterns.PropertyGet (_, propertyInfo, _), Some value ->
            Some (FieldName.create propertyInfo, value)
        | _ ->
            None

    let createEnumByName<'TUnion, 'T> (name: string) (value: 'T) =
        match typeof<'T> with
        | Type.Union ->
            let cases = FSharpType.GetUnionCases(typeof<'TUnion>)
            let case = cases |> Array.tryFind (fun c -> c.Name = name)

            match case with
            | None ->
                Error ReflectionError.FieldNotFound
            | Some case ->
                FSharpValue.MakeUnion(case, [| box value |])
                |> ucast<obj, 'TUnion>
                |> Ok
        | _ ->
            Error ReflectionError.NotUnionType

type Bus = {
    Publish: obj -> Task
}
