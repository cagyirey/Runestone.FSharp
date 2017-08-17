namespace Runestone.FSharp.PortableExecutable

open System
open Runestone.FSharp.NativeInteropHelper

[<Flags>]
type SectionFlags =
| NoPadding =           0x8
| Code =                0x20
| InitializedData =     0x40
| UninitializedData =   0x1000000
| GlobalPtrRelative =   0x2000000
| ExtendedRelocations = 0x4000000
| NotCached =           0x8000000
| SharedMemory =        0x10000000
| Execute =             0x20000000
| Read =                0x40000000
| Write =               0x80000000

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
    Characteristics: SectionFlags
}