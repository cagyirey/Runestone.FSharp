namespace Runestone.FSharp.PortableExecutable

open Runestone.FSharp.NativeInteropHelper

type SectionHeader = {
    Name: string
    VirtualSize: uint32
    VirtualAddress: uint32
    SizeOfRawData: uint32
    PointerToRawData: uint32
    PointerToRelocations: uint32
    PointerToLineNumbers: uint32
    NumberOfRelocations: uint16
    NumberOfLineNumbers: uint16
    Characteristics: uint32 // TOOD: Implement section flags
}
    with
        static member internal FromNative(header: IMAGE_SECTION_HEADER) = {
            Name = header.ManagedName
            VirtualSize = header.VirtualSize
            VirtualAddress = header.VirtualAddress
            SizeOfRawData = header.SizeOfRawData
            PointerToRawData = header.PointerToRawData
            PointerToRelocations = header.PointerToRelocations
            PointerToLineNumbers = header.PointerToLinenumbers
            NumberOfLineNumbers = header.NumberOfLinenumbers
            NumberOfRelocations = header.NumberOfRelocations
            Characteristics = header.Characteristics
    }