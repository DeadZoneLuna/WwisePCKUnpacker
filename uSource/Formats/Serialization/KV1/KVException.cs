using System;

namespace uSource.Formats.Serialization.KV1
{
    public class KVException : Exception
    {
        public KVException(string message)
            : base(message) { }
    }
}