namespace Runestone.FSharp.PortableExecutable

open System
open System.IO
open System.IO.MemoryMappedFiles
open System.Reflection

open Runestone.FSharp.NativeInteropHelper

type PortableExecutableFile private (mmap: MemoryMappedFile) as this =

    let dosHeader, fileHeader, optionalHeader, sectionHeaders = 
        using (mmap.CreateViewAccessor()) (fun reader ->
                
                let dosHeader = DosHeader.FromNative (reader.Read 0L)

                let ntHeaderOffset = int64 dosHeader.NtHeaderOffset
                let optHeaderOffset = ntHeaderOffset + int64 sizeof<IMAGE_FILE_HEADER>

                let fileHeader = FileHeader.FromNative (reader.Read ntHeaderOffset)
                let sectionHeaderOffset = optHeaderOffset + int64 fileHeader.SizeOfOptionalHeader

                let optionalHeader : OptionalHeader = 
                    match fileHeader.SizeOfOptionalHeader with
                    | PE32 -> 
                        reader.Read optHeaderOffset
                        |> OptionalHeader.FromNative32 
                    | PE64 -> 
                        reader.Read optHeaderOffset
                        |> OptionalHeader.FromNative64
                
                let sectionHeaders = 
                    reader.ReadArray(sectionHeaderOffset, int fileHeader.NumberOfSections)
                    |> Array.map (SectionHeader.FromNative)
                    |> Array.toList

                dosHeader, fileHeader, optionalHeader, sectionHeaders
            )
    
    new(path: string) =
        // This ctor should theoretically stop "file is in use by another process" errors
        PortableExecutableFile(MemoryMappedFile.CreateFromFile path)

    new(buffer: byte[]) =
        let mem = MemoryMappedFile.CreateNew(null, buffer.LongLength)
        using(mem.CreateViewStream()) (fun memStream -> memStream.Write(buffer, 0, buffer.Length))
        PortableExecutableFile mem

    new(stream: #Stream) =
        let mem = MemoryMappedFile.CreateNew(null, int64 stream.Length)
        using(mem.CreateViewStream()) (fun memStream -> stream.CopyTo memStream)
        PortableExecutableFile mem

    member x.DosHeader = dosHeader

    member x.FileHeader = fileHeader

    member x.OptionalHeader = optionalHeader

    member x.SectionHeaders = sectionHeaders

    interface IDisposable with
        override x.Dispose () =
            mmap.Dispose()