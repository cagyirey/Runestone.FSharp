namespace rec Runestone.FSharp.PortableExecutable

open System
open System.IO
open System.IO.MemoryMappedFiles
open System.Reflection

open Microsoft.FSharp.NativeInterop

open Runestone.FSharp.NativeInteropHelper

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

type PortableExecutableFile private (mmap: MemoryMappedFile, access : MemoryMappedFileAccess) as this =
    
    let getFileOffset = getFileOffset this
    let findSection = findSection this

    let addVirtualOffset (ptr: nativeptr<_>) offset =
        (NativePtr.toNativeInt ptr) + nativeint (getFileOffset offset)
        |> NativePtr.ofNativeInt<'T>
    
    let dosHeader, fileHeader, optionalHeader, sectionHeaders = 
        using (mmap.CreateViewAccessor(0L, 0L, access)) (fun reader ->
            let dosHeader = mkDosHeader (reader.Read 0L)

            let ntHeaderOffset = int64 dosHeader.NtHeaderOffset
            let optHeaderOffset = ntHeaderOffset + int64 sizeof<IMAGE_FILE_HEADER>

            let fileHeader = mkFileHeader (reader.Read ntHeaderOffset)
            let sectionHeaderOffset = optHeaderOffset + int64 fileHeader.SizeOfOptionalHeader

            let optionalHeader = 
                match fileHeader.SizeOfOptionalHeader with
                | PE32 -> mkOptionalHeader32 (reader.Read optHeaderOffset)
                | PE64 -> mkOptionalHeader64 (reader.Read optHeaderOffset)
                
            let sectionHeaders = 
                reader.ReadArray(sectionHeaderOffset, int fileHeader.NumberOfSections)
                |> Array.map mkSectionHeader
                |> Array.toList

            dosHeader, fileHeader, optionalHeader, sectionHeaders
        )
            
    new(path: string) =
        let fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        let mmf = MemoryMappedFile.CreateFromFile(fileStream, null, 0L, MemoryMappedFileAccess.Read, null, HandleInheritability.None, false)
        PortableExecutableFile(mmf, MemoryMappedFileAccess.Read)
        
    new(buffer: byte[]) =
        let mmf = MemoryMappedFile.CreateNew(null, buffer.LongLength)
        using(mmf.CreateViewStream()) (fun memStream -> memStream.Write(buffer, 0, buffer.Length))
        PortableExecutableFile(mmf, MemoryMappedFileAccess.ReadWrite)

    new(stream: #Stream) =
        let mmf = MemoryMappedFile.CreateNew(null, int64 stream.Length)
        using(mmf.CreateViewStream()) (fun memStream -> stream.CopyTo memStream)
        PortableExecutableFile(mmf, MemoryMappedFileAccess.ReadWrite)

    member x.DosHeader = dosHeader

    member x.FileHeader = fileHeader

    member x.OptionalHeader = optionalHeader

    member x.SectionHeaders = sectionHeaders

    member x.OpenSection(name: string) =
        let section = List.find (fun (section : SectionHeader) -> section.Name = name) sectionHeaders
        mmap.CreateViewStream(int64 section.PointerToRawData, int64 section.SizeOfRawData, access) :> UnmanagedMemoryStream

    member x.OpenSection(section: SectionHeader) =
        let section = List.find ((=) section) sectionHeaders
        mmap.CreateViewStream(int64 section.PointerToRawData, int64 section.SizeOfRawData, access) :> UnmanagedMemoryStream

    member x.TryResolveExports () =
        
        if x.OptionalHeader.ExportTable.VirtualAddress <> 0u then
            use accessor = mmap.CreateViewAccessor(0L, 0L, access)
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

            Some { 
                ModuleName = moduleName
                Timestamp = DateTime.FromUnixTimeSeconds(int64 exportTable.TimeDateStamp)
                Version = Version(int exportTable.MajorVersion, int exportTable.MinorVersion)
                OrdinalBase = exportTable.OrdinalBase
                ExportedFunctions = exports
            }
        else None

    member x.ResolveImports () =
        if x.OptionalHeader.ImportTable.VirtualAddress <> 0u then
            use accessor = mmap.CreateViewAccessor(0L, 0L, access)
        
            let importDirectoryTableOffset = getFileOffset x.OptionalHeader.ImportTable.VirtualAddress
            let importDirectoryTable : IMAGE_IMPORT_DIRECTORY [] = Seq.toArray (accessor.ReadMany importDirectoryTableOffset)

            Array.map (fun (importDirectory : IMAGE_IMPORT_DIRECTORY) ->
                let importLookups = 
                    let offset = getFileOffset importDirectory.ImportLookupTableAddress
                    if x.OptionalHeader.Is64Bit then
                        Array.map mkImportLookup64 (accessor.ReadMany offset)
                    else 
                        Array.map mkImportLookup32 (accessor.ReadMany offset)

                let imports = Array.map(fun import -> 
                    match import with
                    | Choice1Of2 nameRva -> 
                        let offset = getFileOffset nameRva
                        NamedImport {
                            Hint = accessor.ReadUInt16 offset
                            Name = accessor.ReadString(offset + 2L)
                        }
                    | Choice2Of2 ordinal -> ImportByOrdinal ordinal) importLookups

                {
                    ModuleName = accessor.ReadString (getFileOffset importDirectory.AddressOfModuleName)
                    Timestamp = DateTime.FromUnixTimeSeconds (int64 importDirectory.TimeDateStamp)
                    ImportTableAddress = importDirectory.ImportAddressTable
                    ImportedFunctions = imports
                }
            ) importDirectoryTable
        else Array.empty
       
    interface IDisposable with
        override x.Dispose () =
            mmap.Dispose()