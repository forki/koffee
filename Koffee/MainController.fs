﻿namespace Koffee

open FSharp.Desktop.UI

type IPathService =
    abstract Root: Path with get
    abstract Parent: Path -> Path
    abstract GetNodes: Path -> Node list
    abstract OpenFile: Path -> unit
    abstract OpenExplorer: Path -> unit

type MainController(pathing: IPathService) =
    interface IController<MainEvents, MainModel> with
        member this.InitModel model =
            model.Path <- pathing.Root
            model.Nodes <- pathing.GetNodes model.Path

        member this.Dispatcher = function
            | NavUp -> Sync (fun m -> this.NavTo (m.Cursor - 1) m)
            | NavUpHalfPage -> Sync (fun m -> this.NavTo (m.Cursor - m.HalfPageScroll) m)
            | NavDown -> Sync (fun m -> this.NavTo (m.Cursor + 1) m)
            | NavDownHalfPage -> Sync (fun m -> this.NavTo (m.Cursor + m.HalfPageScroll) m)
            | NavToFirst -> Sync (this.NavTo 0)
            | NavToLast -> Sync (fun m -> this.NavTo (m.Nodes.Length - 1) m)
            | PathChanged -> Sync this.OpenPath
            | OpenSelected -> Sync this.SelectedPath
            | OpenParent -> Sync this.ParentPath
            | OpenExplorer -> Sync (fun m -> m.Path |> pathing.OpenExplorer)

    member this.NavTo index (model: MainModel) =
        model.Cursor <- index |> max 0 |> min (model.Nodes.Length - 1)

    member this.OpenPath (model: MainModel) =
        model.Nodes <- pathing.GetNodes model.Path
        model.Cursor <- 0

    member this.SelectedPath (model: MainModel) =
        let path = model.SelectedNode.Path
        match model.SelectedNode.Type with
        | Folder | Drive | Error -> model.Path <- path
        | File -> pathing.OpenFile path

    member this.ParentPath (model: MainModel) =
        model.Path <- pathing.Parent model.Path
