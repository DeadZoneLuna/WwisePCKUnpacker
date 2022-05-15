using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSource.Formats.Serialization.KV1;

namespace WwisePCKUnpacker
{
    public struct FileLUT
    {
        public const uint AK_INVALID_LANGUAGE_ID = 0;

        public UInt64 uFileID;          // File identifier. 
        public uint uBlockSize;         // Required alignment (in bytes).
        public ulong uFileSize;         // File size in bytes. 
        public uint uStartingBlock;     // Start block, from beginning of DATA section. 
        public uint uLanguageID;        // Language ID. AK_INVALID_LANGUAGE_ID if not language-specific. 

        public override string ToString()
        {
            return KVSerializer.Serialize(this);
        }
    }

    public struct StringMap
    {
        public uint id;         // Associated ID.
        public uint offset;     // Byte offset of the beginning of the string relative to the beginning of the map.     
        public string key;      // string

        public string GetKeyName()
        {
            return key.ToLower() == "sfx" ? "all (sfx)" : key;
        }

        public override string ToString()
        {
            return KVSerializer.Serialize(this);
        }
    }

    public struct PackageHeader
    {
        public const uint AK_PACKAGE_ID = ('K' << 24) + ('P' << 16) + ('K' << 8) + 'A';

        public uint HeaderID;           // Header ID. (AKPK)
        public uint m_uHeaderSize;      // Total Header size.
        public uint version;            // AK_FILE_PACKAGE_VERSION (Always 1)
        public uint m_uLangMapsSize;    // Total language map size
        public uint m_uBanksSize;       // Total banks size
        public uint m_uStreamsSize;     // Total streams size
        public uint m_uExternalSize;    // Total externals size

        public override string ToString()
        {
            return KVSerializer.Serialize(this);
        }
    }
}