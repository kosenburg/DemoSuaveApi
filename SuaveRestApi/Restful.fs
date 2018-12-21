module SuaveRestApi.Restful
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Operators
open Suave
open Suave
open Suave
open Suave
open Suave
open Suave.Filters

[<AutoOpen>]
module Restful =
    type RestResource<'a> = {
        GetAll : unit -> 'a seq
        Create : 'a -> 'a
        Update : 'a -> 'a option
        Delete : int -> unit
        GetById: int -> 'a option
        UpdateById: int -> 'a -> 'a option
        IsExists: int -> bool
    }

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getResourceFromReq<'a> (req : HttpRequest) =
    let getString (rawForm : byte []) = System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>

let JSON v =
    let settings = new JsonSerializerSettings()
    settings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

    JsonConvert.SerializeObject(v, settings)
    |> Successful.OK >=> Writers.setMimeType "application/json; charset=utf-8"


let rest resourceName resource =
    
    let resourcePath = "/" + resourceName

    let resourceIdPath =
        let path = resourcePath + "/%d"
        new PrintfFormat<int -> string, unit, string, string, int>(path)

    let deleteResourceById id =
        resource.Delete id
        Successful.NO_CONTENT

    let badRequest = RequestErrors.BAD_REQUEST "Resource not found"
    
    let handleResource requestError = function
        | Some r -> r |> JSON
        | _ -> requestError

    let getResourceById =
        resource.GetById >> handleResource (RequestErrors.NOT_FOUND "Resource not found")
    
    let updateResourceById id =
        request (getResourceFromReq >> (resource.UpdateById id) >> handleResource badRequest)
    
    let getAll = warbler (fun _ -> resource.GetAll() |> JSON)

    let isResourceExists id =
        if resource.IsExists id then Successful.OK "" else RequestErrors.NOT_FOUND ""
    
    choose [
        path resourcePath >=> choose [
            GET >=> getAll
            POST >=> request (getResourceFromReq >> resource.Create >> JSON)
            PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
        ]
        DELETE >=> pathScan resourceIdPath deleteResourceById
        GET >=> pathScan resourceIdPath getResourceById
        PUT >=> pathScan resourceIdPath updateResourceById
        HEAD >=> pathScan resourceIdPath isResourceExists
    ]
