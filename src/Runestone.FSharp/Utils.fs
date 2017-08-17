namespace Runestone.FSharp.PortableExecutable

#nowarn "9"

open System
open System.IO.MemoryMappedFiles
open System.Runtime.InteropServices

open Microsoft.FSharp.NativeInterop

open Runestone.FSharp.NativeInteropHelper

[<AutoOpen>]
module internal Utilities =

    let pe32Size =  sizeof<IMAGE_OPTIONAL_HEADER32>
    let pe64Size = sizeof<IMAGE_OPTIONAL_HEADER64>

    let internal (|PE32|PE64|) (size: uint16) =
        if int size = pe32Size then PE32
        elif int size = pe64Size then PE64
        else invalidArg "fileHeader" "Could not determine the optional header type from its size"

    type DateTime with
        static member FromUnixTimeSeconds(seconds: int64) = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(float seconds)

    type SafeBuffer with
        member x.AcquirePointer () =
            let ptr = ref (NativePtr.ofNativeInt 0n)
            x.AcquirePointer ptr
            !ptr

    type MemoryMappedViewAccessor with

        member x.Read<'T>(position: int64) =
            let structure = ref Unchecked.defaultof<'T>
            x.Read(position, structure)
            !structure

        member x.ReadArray<'T>(position: int64, count) =
            let structures : 'T [] = Array.zeroCreate count
            x.ReadArray(position, structures, 0, count) |> ignore
            structures

        member x.ReadString(position: int64) =
            let ptr = NativePtr.toNativeInt (x.SafeMemoryMappedViewHandle.AcquirePointer())
            String(NativePtr.ofNativeInt<sbyte>(ptr + nativeint position))

        member x.ReadMany<'T>(position: int64) =
            let structure = ref Unchecked.defaultof<'T>
            Seq.unfold (fun offset -> 
                let structure : 'T = x.Read offset
                if structure = Unchecked.defaultof<'T> then None
                else Some(structure, offset + int64 sizeof<'T>)) position
            |> Seq.toArray


            