namespace Runestone.FSharp.PortableExecutable

open System

type ImportHint = {
    Hint: uint16
    Name: string
}

type ImportedFunction =
    | ImportByOrdinal of uint16
    | NamedImport of ImportHint

type ImportDirectory = {
    ModuleName: string
    Timestamp: DateTime
    ImportTableAddress: uint32
    ImportedFunctions: ImportedFunction []
}