namespace App.Components

open App.Stores
open Feliz
open MobF.React

[<RequireQualifiedAccess>]
module PlayerControls =
    
    [<ObserverComponent>]
    let comp (props: {| PlayerStore: PlayerStore.Model |}) =
        
        let playerRef = React.useRef<Browser.Types.HTMLAudioElement option>(None)
        React.useEffect((fun () ->
            match playerRef.current with
            | None -> ()
            | Some playerRef ->
                playerRef.volume <- props.PlayerStore.State.Volume
            
            ), [| box playerRef |])
        
        Html.div [
            Html.h3 (sprintf "Volume: %f" props.PlayerStore.State.Volume)
            Html.audio [
                prop.onVolumeChange (fun e ->
                        let target = e.target :?> Browser.Types.HTMLAudioElement
                        props.PlayerStore.Post(target.volume |> PlayerStore.VolumeChange)
                )

                prop.ref playerRef
                prop.controls true
                prop.id "main-player"
                prop.children [
                    Html.source [ prop.src "uno.m4a" ]
                ]
            ]
        ]