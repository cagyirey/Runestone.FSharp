using System;
using System.Runtime.InteropServices;

namespace Runestone.FSharp.NativeInteropHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct IMAGE_FILE_HEADER {
        public fixed byte Signature[4];

        public UInt16 Machine;
        public UInt16 NumberOfSections;
        public UInt32 TimeDateStamp;
        public UInt32 PointerToSymbolTable;
        public UInt32 NumberOfSymbols;
        public UInt16 SizeOfOptionalHeader;
        public UInt16 Characteristics;

        public string ManagedSignature
        {
            get
            {
                fixed (byte* pSignature = this.Signature)
                {
                    return new string((sbyte*)pSignature, 0, 4);
                }
            }
        }

    }
}
