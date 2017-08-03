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
    with
        static member internal FromNative(header: IMAGE_DOS_HEADER) = {
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