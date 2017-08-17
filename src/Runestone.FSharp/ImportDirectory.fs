namespace Runestone.FSharp.PortableExecutable

open System

type ImportDirectory = {
    ModuleName: string
    Timestamp: DateTime
    ImportTableAddress: uint32
    ImportLookups: Choice<uint32 [], uint64 []>
}