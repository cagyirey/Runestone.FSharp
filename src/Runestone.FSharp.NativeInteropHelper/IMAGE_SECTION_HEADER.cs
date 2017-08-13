using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Runestone.FSharp.NativeInteropHelper
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IMAGE_SECTION_HEADER
    {
        public fixed byte Name[8];
        public UInt32 VirtualSize;
        public UInt32 VirtualAddress;
        public UInt32 SizeOfRawData;
        public UInt32 PointerToRawData;
        public UInt32 PointerToRelocations;
        public UInt32 PointerToLinenumbers;
        public UInt16 NumberOfRelocations;
        public UInt16 NumberOfLinenumbers;
        public UInt32 Characteristics;

        public string ManagedName { 
            get
            {
                fixed (byte* pName = this.Name)
                {
                    return new string((sbyte*)pName, 0, 8, Encoding.UTF8).TrimEnd('\x0');
                }
            }
        }
    }
}
