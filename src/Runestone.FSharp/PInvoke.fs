namespace rec Runestone.FSharp

open System
open System.Runtime.InteropServices

open Microsoft.Win32
open Microsoft.FSharp.NativeInterop
open Microsoft.Win32.SafeHandles

// avoid naming conflicts with managed wrappers
module private PInvoke =

    open Kernel32

    [<DllImport("kernel32.dll")>]
    extern nativeint OpenProcess(ProcessRights dwDesiredAccess, bool bInheritHandle, int dwProcessId)

    [<DllImport("kernel32.dll")>]
    extern bool ReadProcessMemory(nativeint hProcess, nativeint lpBaseAddress, nativeint lpBuffer, nativeint nSize, nativeint* lpNumberOfBytesRead)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Kernel32 =

    [<Flags>]
    type ProcessRights =
    // Standard security rights
    | Delete        = 0x00010000
    | ReadControl   = 0x00020000
    | Synchronize   = 0x00100000
    | WriteDacl     = 0x00040000
    | WriteOwner    = 0x00080000
    // Process-specific rights
    | CreateProcess             = 0x0080
    | CreateThread              = 0x0002
    | DuplicateHandle           = 0x0040
    | QueryInformation          = 0x0400
    | QueryLimitedInformation   = 0x1000
    | SetInformation            = 0x0200
    | SetQuota                  = 0x0100
    | SuspendResume             = 0x0800
    | Terminate                 = 0x0001
    | VirtualMemoryOperation    = 0x0008
    | VirtualMemoryRead         = 0x0010
    | VirtualMemoryWrite        = 0x0020
    | Sychronize                = 0x00100000
    
type Kernel32 private () =

    static member ReadProcessMemory(hProcess, baseAddress, buffer: byte[], numBytes) =
        let pBuffer = fixed buffer
        let mutable bytesRead = 0n
        if PInvoke.ReadProcessMemory(hProcess, baseAddress, NativePtr.toNativeInt pBuffer, numBytes, &&bytesRead) then
            int64 bytesRead
        else
            Marshal.GetHRForLastWin32Error()
            |> Marshal.GetExceptionForHR
            |> raise

    static member ReadProcessMemory(hProcess, baseAddress, buffer: nativeptr<byte>, numBytes) =
        let mutable bytesRead = 0n
        if PInvoke.ReadProcessMemory(hProcess, baseAddress, NativePtr.toNativeInt buffer, numBytes, &&bytesRead) then
            int64 bytesRead
        else
            Marshal.GetHRForLastWin32Error()
            |> Marshal.GetExceptionForHR
            |> raise

    //static member ReadProcessMemory<'T when 'T : unmanaged>(hProcess, baseAddress, buffer: nativeptr<'T>, count) =
    //    let structSize = sizeof<'T>
    //    let mutable bytesRead = 0n
    //    if PInvoke.ReadProcessMemory(hProcess, baseAddress, NativePtr.toNativeInt buffer, nativeint (structSize * count), &&bytesRead) then
    //        int64 (bytesRead / nativeint structSize)
    //    else
    //        Marshal.GetHRForLastWin32Error()
    //        |> Marshal.GetExceptionForHR
    //        |> raise
            
    static member ReadProcessMemory<'T when 'T : unmanaged>(hProcess, baseAddress) =
        let structSize = sizeof<'T>
        let mutable structure = Unchecked.defaultof<'T>
        let mutable bytesRead = 0n
        if PInvoke.ReadProcessMemory(hProcess, baseAddress, NativePtr.toNativeInt &&structure, nativeint structSize, &&bytesRead) then
            structure
        else
            Marshal.GetHRForLastWin32Error()
            |> Marshal.GetExceptionForHR
            |> raise

    static member OpenProcess(processRights, processId) =
        new SafeProcessHandle(PInvoke.OpenProcess(processRights, false, processId), true)



    
        

    