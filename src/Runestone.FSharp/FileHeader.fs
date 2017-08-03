namespace Runestone.FSharp.PortableExecutable

open System

open Runestone.FSharp.NativeInteropHelper

type MachineKind = 
    | Unknown = 0x0us
    | AM33 = 0x1d3us
    | AMD64 = 0x8664us
    | ARM = 0x1c0us
    | ARM64 = 0xaa64us
    | Thumb2 = 0x1c4us
    | EfiByteCode = 0xebcus
    | I386 = 0x14cus
    | IA64 = 0x200us
    | M32R = 9041us
    | Mips16 = 0x266us
    | MipsFpu = 0x366us
    | MipsFpu16 = 0x466us
    | PowerPC = 0x1f0us
    | PowerPCFloatingPoint = 0x1f1us // Todo: something less ugly
    | R4000 = 0x166us
    | RiscV32 = 0x5032us
    | RiscV64 = 0x5064us
    | RiscV128 = 0x5128us
    | SH3 = 0x1a2us
    | SH3Dsp = 0x1a3us
    | SH4 = 0x1a6us
    | SH5 = 0x1a8us
    | Thumb = 0x1c2us
    | MipsWceV2 = 0x169us

[<Flags>]
type FileCharacteristics =
    | RelocationsStripped =         0x1us
    | ExecutableImage =             0x2us
    | LineNumbersStripped =         0x4us
    | SymbolsStripped =             0x8us
    | AggressiveWorkingSetTrim =    0x10us
    | LargeAddressAware =           0x20us
    | LittleEndian =                0x80us
    | ``32BitMachine`` =            0x100us
    | DebugStripped =               0x200us
    | RemovableRunFromSwap =        0x400us
    | NetRunFromSwap =              0x800us
    | System =                      0x1000us
    | Dll =                         0x2000us
    | UniprocessorOnly =            0x4000us
    | BigEndian =                   0x8000us

type FileHeader = {
    Signature: string
    Machine: MachineKind
    NumberOfSections: uint16
    DatetimeStamp: DateTime
    PointerToSymbolTable: uint32
    NumberOfSymbols: uint32
    SizeOfOptionalHeader: uint16
    Characteristics: FileCharacteristics
}
    with
        static member internal FromNative(header: IMAGE_FILE_HEADER) = {
            Signature = header.ManagedSignature
            Machine = LanguagePrimitives.EnumOfValue header.Machine
            NumberOfSections = header.NumberOfSections
            DatetimeStamp = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(float header.TimeDateStamp)
            PointerToSymbolTable = header.PointerToSymbolTable
            NumberOfSymbols = header.NumberOfSymbols
            SizeOfOptionalHeader = header.SizeOfOptionalHeader
            Characteristics = LanguagePrimitives.EnumOfValue header.Characteristics
        }