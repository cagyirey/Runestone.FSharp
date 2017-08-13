using System;
using System.Runtime.InteropServices;

namespace Runestone.FSharp.NativeInteropHelper
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_IMPORT_DIRECTORY {
        public UInt32 ImportLookupTableAddress;
        public UInt32 TimeDateStamp;
        public UInt32 ForwarderChain;
        public UInt32 AddressOfModuleName;
        public UInt32 ImportAddressTable;
    }
}
