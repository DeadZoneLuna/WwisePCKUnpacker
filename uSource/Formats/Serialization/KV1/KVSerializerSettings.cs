namespace uSource.Formats.Serialization.KV1
{
    public class KVSerializerSettings
    {
        public static KVSerializerSettings Default
        {
            get
            {
                return new KVSerializerSettings();
            }
        }

        public static KVSerializerSettings Common
        {
            get
            {
                return new KVSerializerSettings
                {
                    ToLowerCase = true,
                    UsesEscapeSequences = true,
                    UsesConditionals = true
                };
            }
        }

        /// <summary>
        /// Ignore token deserialization for <see cref="KVReader.State.Object"/>. (Can be used for entities deserialization!)
        /// </summary>
        public bool IgnoreToken = false;

        /// <summary>
        /// Deserialize chars to lowercase.
        /// </summary>
        public bool ToLowerCase = false;

        /// <summary>
        /// Determines whether the parser should translate escape sequences (/n, /t, etc.).
        /// </summary>
        public bool UsesEscapeSequences = false;

        /// <summary>
        /// Determines whether the parser should evaluate conditional blocks ([$WINDOWS], etc.).
        /// </summary>
        public bool UsesConditionals = true;

        /// <summary>
        /// Sets the size of the token buffer used for deserialization.
        /// </summary>
        public int MaximumTokenSize = 4096;

        //TODO: System information
        public bool IsXBox360 = false, IsWin32 = true;
        public bool IsWindows = true, IsOSX = false, IsLinux = false, IsPosix = false;
    }
}