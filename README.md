# Runestone.FSharp

A library for analyzing portable executable files. This is a very rough draft.

### Usage

```fsharp
#r "Runestone.FSharp.dll"

open Runestone.FSharp.PortableExecutable

let path = @"###PATH_TO_EXECUTABLE###"

let binary = PortableExecutableFile path

let headers =
    ["DOS header", box binary.DosHeader
     "File header", box binary.FileHeader
     "Optional header", box binary.OptionalHeader
     "Section headers", box binary.SectionHeaders]

for (title, header) in headers do
    printfn "---------------\r\n%s\r\n--------------\r\n%A\r\n" title header
```