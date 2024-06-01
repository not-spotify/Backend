namespace App.Components

open Elmish
open Feliz
open Feliz.Router

open App.Stores

module RootRouter =
    
    type State = { CurrentUrl : string list }
    
    let playerStore = PlayerStore.create()

    type Msg =
        | UrlChanged of string list
        | NavigateToUsers
        | NavigateToUser of int
    
    let init() = { CurrentUrl = Router.currentUrl() }, Cmd.none

    let update (msg: Msg) (state: State, _) =
        match msg with
        // notice here the use of the command Cmd.navigate
        | NavigateToUsers ->
            state, Cmd.navigate("users")
        | UrlChanged segments -> { state with CurrentUrl = segments }, Cmd.none
        // Router.navigate with query string parameters
        | NavigateToUser userId -> state, Cmd.navigate("users", [ "id", userId ])

    let render (state, _) dispatch =
        let currentPage =
            match state.CurrentUrl with
            | [ ] ->
                Html.div [
                    Html.h1 "Home"
                    Html.button [
                        prop.text "Navigate to users"
                        prop.onClick (fun _ -> dispatch NavigateToUsers)
                    ]
                    Html.div [
                        PlayerControls.comp({| PlayerStore = playerStore |})
                    ]
                    
                    Html.a [
                        prop.href (Router.format("users"))
                        prop.text "Users link"
                    ]
                ]
            | [ "users" ] ->
                Html.div [
                    Html.h1 "Users page"
                    Html.button [
                        prop.text "Navigate to User(10)"
                        prop.onClick (fun _ -> dispatch (NavigateToUser 10))
                    ]
                    Html.a [
                        prop.href (Router.format("users", ["id", 10]))
                        prop.text "Single User link"
                    ]
                ]

            | [ "users"; Route.Query [ "id", Route.Int userId ] ] ->
                Html.h1 (sprintf "Showing user %d" userId)

            | _ ->
                Html.h1 "Not found"

        React.router [
            router.onUrlChanged (UrlChanged >> dispatch)
            router.children currentPage
        ]
