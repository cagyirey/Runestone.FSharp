﻿namespace Runestone.FSharp.PortableExecutable

open System

open Runestone.FSharp.NativeInteropHelper

type Subsystem =
    | Unknown = 0
    | Native = 1
    | WindowsGui = 2
    | WindowsCui = 3
    | OS2Cui = 5
    | PosixCui = 7
    | NativeWindows = 8
    | WindowsCE = 9
    | EfiApplication = 10
    | EfiBootServiceDriver = 11
    | EfiRuntimeDriver = 12
    | EfiRom = 13
    | Xbox = 14
    | WindowsBootApplication = 16

[<Flags>]
type DllCharacteristics =
    | HighEntropyVirtualAddresses = 0x20
    | DynamicBase = 0x40
    | ForceIntegrity = 0x80
    | NoExecuteCompatible = 0x100
    | NoIsolation = 0x200
    | NoSEH = 0x400
    | NoBind = 0x800
    | AppContainer = 0x1000
    | WdmDriver = 0x2000
    | ControlFlowGuard = 0x4000
    | TerminalServerAware = 0x8000

type OptionalHeader = {
    LinkerVersion: Version
    OperatingSystemVersion: Version
    ImageVersion: Version
    SubsystemVersion: Version
    Win32VersionValue: uint32

    SizeOfCode: uint32
    SizeOfInitializedData: uint32
    SizeOfUnitializedData: uint32
    AddressOfEntryPoint: uint32
    BaseOfCode: uint32
    BaseOfData: uint32 option
    ImageBase: uint64
    SectionAlignment: uint32
    FileAlignment: uint32
    SizeOfImage: uint32
    SizeOfHeader: uint32
    Checksum: uint32
    Subsystem: Subsystem
    DllCharacteristics: DllCharacteristics
    SizeOfStackReserve: uint64
    SizeOfStackCommit: uint64
    SizeOfHeapReserve: uint64
    SizeOfheapCommit: uint64
    LoaderFlags: uint32
    NumberOfRvaAndSizes: uint32

}
    with
        member x.Is64Bit =
            match x.BaseOfData with
            | Some _ -> false
            | None -> true

        static member internal FromNative32(header: IMAGE_OPTIONAL_HEADER32) = {
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
        }

        static member internal FromNative64(header: IMAGE_OPTIONAL_HEADER64) = {
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
        }
            