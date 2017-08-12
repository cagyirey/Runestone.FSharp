namespace Runestone.FSharp.PortableExecutable

open System

type ExportedFunction =
| VirtualAddress
| 

type ExportDirectory = {
    Timestamp: DateTime
    Version: Version
    ModuleName: string
    OrdinalBase: uint16

}