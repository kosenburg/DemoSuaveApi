// Learn more about F# at http://fsharp.org

open Suave
open SuaveRestApi
open SuaveRestApi.Db
open SuaveRestApi.Db
open SuaveRestApi.Db
open SuaveRestApi.Db
open System

[<EntryPoint>]
let main argv =
    let personWebPart = Restful.rest "people" {
        GetAll = Db.getPeople
        Create = Db.createPerson
        Update = Db.updatePerson
        Delete = Db.deletePerson
        GetById = Db.getPerson
        UpdateById = Db.updatePersonById
        IsExists = Db.isPersonExists
    }

    Web.startWebServer defaultConfig personWebPart
        
    0 // return an integer exit code
