namespace rec Runestone.FSharp.PortableExecutable

open System
open System.IO
open System.IO.MemoryMappedFiles
open System.Reflection

open Runestone.FSharp.NativeInteropHelper
open Microsoft.FSharp.NativeInterop

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PortableExecutableFile =

    let tryFindSection (file: PortableExecutableFile) virtualAddress =
       file.SectionHeaders
       |> List.tryFind(fun (section : SectionHeader) -> virtualAddress >= section.VirtualAddress && virtualAddress < section.VirtualAddress + section.VirtualSize)
    
    let findSection (file: PortableExecutableFile) virtualAddress =
       file.SectionHeaders
       |> List.find(fun (section : SectionHeader) -> virtualAddress >= section.VirtualAddress && virtualAddress < section.VirtualAddress + section.VirtualSize)

    let inline getFileOffset (file: PortableExecutableFile) virtualAddress : int64 = 
       let section = findSection file virtualAddress
       int64 (virtualAddress - section.VirtualAddress + section.PointerToRawData)

type PortableExecutableFile private (mmap: MemoryMappedFile) as this =
    
    let getFileOffset = getFileOffset this
    let findSection = findSection this

    let addVirtualOffset (ptr: nativeptr<_>) offset =
        (NativePtr.toNativeInt ptr) + nativeint (getFileOffset offset)
        |> NativePtr.ofNativeInt<'T>
    
    let dosHeader, fileHeader, optionalHeader, sectionHeaders = 
        using (mmap.CreateViewAccessor()) (fun reader ->
            let dosHeader = DosHeader.FromNative (reader.Read 0L)

            let ntHeaderOffset = int64 dosHeader.NtHeaderOffset
            let optHeaderOffset = ntHeaderOffset + int64 sizeof<IMAGE_FILE_HEADER>

            let fileHeader = FileHeader.FromNative (reader.Read ntHeaderOffset)
            let sectionHeaderOffset = optHeaderOffset + int64 fileHeader.SizeOfOptionalHeader

            let optionalHeader = 
                match fileHeader.SizeOfOptionalHeader with
                | PE32 -> OptionalHeader.FromNative32 (reader.Read optHeaderOffset)
                | PE64 -> OptionalHeader.FromNative64 (reader.Read optHeaderOffset)
                
            let sectionHeaders = 
                reader.ReadArray(sectionHeaderOffset, int fileHeader.NumberOfSections)
                |> Array.map (SectionHeader.FromNative)
                |> Array.toList

            dosHeader, fileHeader, optionalHeader, sectionHeaders
        )
            
    new(path: string) =
        // BUG: "file is in use by another process" - should be solved with read only permissions but FileAccess.Read causes UnauthorizedAccessException
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

    member x.ResolveExports () =
        use accessor = mmap.CreateViewAccessor()
        let exportTable : IMAGE_EXPORT_DIRECTORY = accessor.Read (getFileOffset x.OptionalHeader.ExportTable.VirtualAddress)

        let pImageData = accessor.SafeMemoryMappedViewHandle.AcquirePointer ()
        let moduleName = String(addVirtualOffset<_, sbyte> pImageData exportTable.AddressOfModuleName)
            
        let exportRvas : uint32 [] = accessor.ReadArray(getFileOffset exportTable.AddressOfFunctions, int exportTable.NumberOfFunctions)
        let nameRvas : uint32 [] = accessor.ReadArray(getFileOffset exportTable.AddressOfNames, int exportTable.NumberOfNames)
        let ordinals : uint16 [] = accessor.ReadArray(getFileOffset exportTable.AddressOfNameOrdinals, int exportTable.NumberOfNames)

        let names = Array.map(fun rva -> String(addVirtualOffset<_, sbyte> pImageData rva)) nameRvas
        let nameOrdinalPairs = Array.zip ordinals names |> Map
        
        let exports = 
            Array.mapi(fun i functionRva ->
                {
                    Name = Map.tryFind (uint16 i) nameOrdinalPairs 
                    VirtualAddress = functionRva
                    Ordinal = uint16 exportTable.OrdinalBase + uint16 i
                }
            ) exportRvas

        { 
            ModuleName = moduleName
            Timestamp = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(float exportTable.TimeDateStamp)
            Version = Version(int exportTable.MajorVersion, int exportTable.MinorVersion)
            OrdinalBase = exportTable.OrdinalBase
            ExportedFunctions = exports
        }

    interface IDisposable with
        override x.Dispose () =
            mmap.Dispose()