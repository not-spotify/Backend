namespace App.Stores

open MobF

module PlayerStore =
    type State = {
        Paused: bool
        Volume: float
    }
    
    type Msg =
        | VolumeChange of volume: float
    
    
    type Model = Model<State, Msg>
    
    let create () =
        let init () = {
            Paused = true
            Volume = 0.2
        }
        
        let update state = function
            | VolumeChange vol -> { state with Volume = vol }
        
        Model.useInit init
        |> Model.andUpdate update
        |> Model.create
        
    