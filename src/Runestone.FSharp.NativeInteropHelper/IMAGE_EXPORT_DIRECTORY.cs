using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runestone.FSharp.NativeInteropHelper
{
    internal struct IMAGE_EXPORT_DIRECTORY
    {
        internal UInt32 m_Characteristics;
        internal UInt32 m_TimeDateStamp;
        internal UInt16 m_MajorVersion;
        internal UInt16 m_MinorVersion;
        internal UInt32 m_AddressOfModuleName;
        internal UInt32 m_OrdinalBase;
        internal UInt32 m_NumberOfFunctions;
        internal UInt32 m_NumberOfNames;
        internal UInt32 m_AddressOfFunctions;     // RVA from base of image
        internal UInt32 m_AddressOfNames;         // RVA from base of image
        internal UInt32 m_AddressOfNameOrdinals;  // RVA from base of image
    }
}
