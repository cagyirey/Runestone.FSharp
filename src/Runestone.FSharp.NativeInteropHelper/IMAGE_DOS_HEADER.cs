using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Runestone.FSharp.NativeInteropHelper
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IMAGE_DOS_HEADER {      
        public fixed byte e_magic[2];       // Magic number
        public UInt16 e_cblp;               // Bytes on last page of file
        public UInt16 e_cp;                 // Pages in file
        public UInt16 e_crlc;               // Relocations
        public UInt16 e_cparhdr;            // Size of header in paragraphs
        public UInt16 e_minalloc;           // Minimum extra paragraphs needed
        public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
        public UInt16 e_ss;                 // Initial (relative) SS value
        public UInt16 e_sp;                 // Initial SP value
        public UInt16 e_csum;               // Checksum
        public UInt16 e_ip;                 // Initial IP value
        public UInt16 e_cs;                 // Initial (relative) CS value
        public UInt16 e_lfarlc;             // File address of relocation table
        public UInt16 e_ovno;               // Overlay number
        public fixed UInt16 e_res[4];       // Reserved words
        public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
        public UInt16 e_oeminfo;            // OEM information; e_oemid specific
        public fixed UInt16 e_res2[10];     // Reserved words
        public UInt32 e_lfanew;             // File address of new exe header

        public string ManagedSignature
        {
            get
            {
                fixed (byte* pSignature = this.e_magic)
                {
                    return new string((sbyte*)pSignature, 0, 2);
                }
            }
        }
    }


}
