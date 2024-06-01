module Main

open Elmish
open Elmish.React

open App.Components

Fable.Core.JsInterop.importSideEffects "./Styles/main.scss"

Program.mkSimple RootRouter.init RootRouter.update RootRouter.render
|> Program.withReactSynchronous "fsharp-rocks-mount"
|> Program.run
