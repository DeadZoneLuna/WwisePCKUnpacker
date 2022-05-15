using System.Collections.Generic;

namespace uSource.Formats.Serialization.KV1
{
    public interface IVEnumerable<T> : IEnumerable<T> where T : VToken
    {
        IVEnumerable<VToken> this[object key] { get; }
    }
}