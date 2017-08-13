namespace Runestone.FSharp.PortableExecutable

open System

type ExportedFunction = {
    VirtualAddress: uint32
    Ordinal: uint16
    Name: string option
}

type ExportDirectory = {
    Timestamp: DateTime
    Version: Version
    ModuleName: string
    OrdinalBase: uint32
    ExportedFunctions: ExportedFunction []
}