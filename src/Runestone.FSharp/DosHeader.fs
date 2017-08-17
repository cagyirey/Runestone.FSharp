namespace Runestone.FSharp.PortableExecutable

open System
open Runestone.FSharp.NativeInteropHelper

type DosHeader = {
    MagicBytes: string
    BytesOnLastPage: uint16
    PageCount: uint16
    RelocationCount: uint16
    MinimumParagraphs: uint16
    MaximumParagraphs: uint16
    InitialSsValue: uint16
    InitialSpValue: uint16
    Checksum: uint16
    InitialIpValue: uint16
    InitialCsValue: uint16
    RelocationsOffset: uint16
    Overlay: uint16
    OemId: uint16
    OemInfo: uint16
    NtHeaderOffset: uint32
}