namespace Runestone.FSharp.PortableExecutable

open System
open Runestone.FSharp.NativeInteropHelper

[<AutoOpen>]
module internal TypeConversions =

    let mkDosHeader (header: IMAGE_DOS_HEADER) = {
            MagicBytes = header.ManagedSignature
            BytesOnLastPage = header.e_cblp
            PageCount = header.e_cp
            RelocationCount = header.e_crlc
            MinimumParagraphs = header.e_minalloc
            MaximumParagraphs = header.e_maxalloc
            InitialSsValue = header.e_ss
            InitialSpValue = header.e_sp
            InitialIpValue = header.e_ip
            InitialCsValue = header.e_cs
            Checksum = header.e_csum
            RelocationsOffset = header.e_lfarlc
            Overlay = header.e_ovno
            OemId = header.e_oemid
            OemInfo = header.e_oeminfo
            NtHeaderOffset = header.e_lfanew
    }

    let mkFileHeader (header: IMAGE_FILE_HEADER) = {
        Signature = header.ManagedSignature
        Machine = LanguagePrimitives.EnumOfValue header.Machine
        NumberOfSections = header.NumberOfSections
        DatetimeStamp = DateTime.FromUnixTimeSeconds (int64 header.TimeDateStamp)
        PointerToSymbolTable = header.PointerToSymbolTable
        NumberOfSymbols = header.NumberOfSymbols
        SizeOfOptionalHeader = header.SizeOfOptionalHeader
        Characteristics = LanguagePrimitives.EnumOfValue header.Characteristics
    }

    let mkSectionHeader (header: IMAGE_SECTION_HEADER) = {
        Name = header.ManagedName
        VirtualSize = header.VirtualSize
        VirtualAddress = header.VirtualAddress
        SizeOfRawData = header.SizeOfRawData
        PointerToRawData = header.PointerToRawData
        PointerToRelocations = header.PointerToRelocations
        PointerToLineNumbers = header.PointerToLinenumbers
        NumberOfLineNumbers = header.NumberOfLinenumbers
        NumberOfRelocations = header.NumberOfRelocations
        Characteristics = LanguagePrimitives.EnumOfValue header.Characteristics
    }

    let mkDataDirectory (header: IMAGE_DATA_DIRECTORY) = {
        VirtualAddress = header.VirtualAddress
        Size = header.Size
    }
    
    let mkImportLookup64 (lookup: uint64) =
        let importByName = (lookup &&& 0x8000000000000000UL) = 0UL
        if importByName then Choice1Of2 (uint32 lookup)
        else Choice2Of2 (uint16 lookup)

    let mkImportLookup32 (lookup: uint32) =
        let importByName = (lookup &&& 0x80000000u) = 0u
        if importByName then Choice1Of2 lookup
        else Choice2Of2 (uint16 lookup)

    let mkOptionalHeader32 (header: IMAGE_OPTIONAL_HEADER32) = {
        LinkerVersion = Version(int header.MajorLinkerVersion, int header.MinorLinkerVersion)
        OperatingSystemVersion = Version(int header.MajorOperatingSystemVersion, int header.MinorOperatingSystemVersion)
        ImageVersion = Version(int header.MajorImageVersion, int header.MinorImageVersion)
        SubsystemVersion = Version(int header.MajorSubsystemVersion, int header.MinorSubsystemVersion)
        Win32VersionValue = header.Win32VersionValue

        SizeOfCode = header.SizeOfCode
        SizeOfInitializedData = header.SizeOfInitializedData
        SizeOfUnitializedData = header.SizeOfUninitializedData
        AddressOfEntryPoint = header.AddressOfEntryPoint
        BaseOfCode = header.BaseOfCode
        BaseOfData = Some header.BaseOfData
        ImageBase = uint64 header.ImageBase
        SectionAlignment = header.SectionAlignment
        FileAlignment = header.FileAlignment
        SizeOfImage = header.SizeOfImage
        SizeOfHeader = header.SizeOfHeaders
        Checksum = header.CheckSum
        Subsystem = enum<Subsystem> (int header.Subsystem)
        DllCharacteristics = enum<DllCharacteristics> (int header.DllCharacteristics)
        SizeOfStackReserve = uint64 header.SizeOfStackReserve
        SizeOfStackCommit = uint64 header.SizeOfStackCommit
        SizeOfHeapReserve = uint64 header.SizeOfHeapReserve
        SizeOfheapCommit = uint64 header.SizeOfHeapReserve
        LoaderFlags = header.LoaderFlags
        NumberOfRvaAndSizes = header.NumberOfRvaAndSizes

        ExportTable = mkDataDirectory header.ExportTable
        ImportTable = mkDataDirectory header.ImportTable
        ResourceTable = mkDataDirectory header.ResourceTable
        ExceptionTable = mkDataDirectory header.ExceptionTable
        CertificateTable = mkDataDirectory header.CertificateTable
        BaseRelocationTable = mkDataDirectory header.BaseRelocationTable
        Debug = mkDataDirectory header.Debug
        Architecture = mkDataDirectory header.Architecture
        GlobalPtr = mkDataDirectory header.GlobalPtr
        TlsTable = mkDataDirectory header.TLSTable
        LoadConfigTable = mkDataDirectory header.LoadConfigTable
        BoundImport = mkDataDirectory header.BoundImport
        ImportAddressTable = mkDataDirectory header.IAT
        DelayImportDescriptor = mkDataDirectory header.DelayImportDescriptor
        ClrRuntimeHeader = mkDataDirectory header.CLRRuntimeHeader
    }

    let mkOptionalHeader64(header: IMAGE_OPTIONAL_HEADER64) = {
        LinkerVersion = Version(int header.MajorLinkerVersion, int header.MinorLinkerVersion)
        OperatingSystemVersion = Version(int header.MajorOperatingSystemVersion, int header.MinorOperatingSystemVersion)
        ImageVersion = Version(int header.MajorImageVersion, int header.MinorImageVersion)
        SubsystemVersion = Version(int header.MajorSubsystemVersion, int header.MinorSubsystemVersion)
        Win32VersionValue = header.Win32VersionValue

        SizeOfCode = header.SizeOfCode
        SizeOfInitializedData = header.SizeOfInitializedData
        SizeOfUnitializedData = header.SizeOfUninitializedData
        AddressOfEntryPoint = header.AddressOfEntryPoint
        BaseOfCode = header.BaseOfCode
        BaseOfData = None
        ImageBase = header.ImageBase
        SectionAlignment = header.SectionAlignment
        FileAlignment = header.FileAlignment
        SizeOfImage = header.SizeOfImage
        SizeOfHeader = header.SizeOfHeaders
        Checksum = header.CheckSum
        Subsystem = enum<Subsystem> (int header.Subsystem)
        DllCharacteristics = enum<DllCharacteristics> (int header.DllCharacteristics)
        SizeOfStackReserve = header.SizeOfStackReserve
        SizeOfStackCommit = header.SizeOfStackCommit
        SizeOfHeapReserve = header.SizeOfHeapReserve
        SizeOfheapCommit = header.SizeOfHeapReserve
        LoaderFlags = header.LoaderFlags
        NumberOfRvaAndSizes = header.NumberOfRvaAndSizes

        ExportTable = mkDataDirectory header.ExportTable
        ImportTable = mkDataDirectory header.ImportTable
        ResourceTable = mkDataDirectory header.ResourceTable
        ExceptionTable = mkDataDirectory header.ExceptionTable
        CertificateTable = mkDataDirectory header.CertificateTable
        BaseRelocationTable = mkDataDirectory header.BaseRelocationTable
        Debug = mkDataDirectory header.Debug
        Architecture = mkDataDirectory header.Architecture
        GlobalPtr = mkDataDirectory header.GlobalPtr
        TlsTable = mkDataDirectory header.TLSTable
        LoadConfigTable = mkDataDirectory header.LoadConfigTable
        BoundImport = mkDataDirectory header.BoundImport
        ImportAddressTable = mkDataDirectory header.IAT
        DelayImportDescriptor = mkDataDirectory header.DelayImportDescriptor
        ClrRuntimeHeader = mkDataDirectory header.CLRRuntimeHeader
    }