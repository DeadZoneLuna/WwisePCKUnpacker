using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using uSource.Formats.Utilities;

namespace uSource.Formats.Serialization.KV1
{
    public abstract class VToken : IVEnumerable<VToken>
    {
        // TODO: Implement these.
        public VToken Parent { get; internal set; }
        public VToken Previous { get; internal set; }
        public VToken Next { get; internal set; }

        public abstract void WriteTo(KVWriter writer);

        public abstract VTokenType Type { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<VToken>)this).GetEnumerator();
        }

        IEnumerator<VToken> IEnumerable<VToken>.GetEnumerator()
        {
            return Children().GetEnumerator();
        }

        IVEnumerable<VToken> IVEnumerable<VToken>.this[object key]
        {
            get
            {
                VToken value = this[key];
                return value != null ? value : null;
            }
        }

        public static bool DeepEquals(VToken t1, VToken t2)
        {
            return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
        }

        public abstract VToken DeepClone();

        public virtual VToken this[object key]
        {
            get
            {
                throw new InvalidOperationException(string.Format("Cannot access child value on {0}.", GetType()));
            }

            set
            {
                throw new InvalidOperationException(string.Format("Cannot set child value on {0}.", GetType()));
            }
        }

        public virtual T Value<T>(object key) where T : VToken
        {
            VToken token = this[key];
            return token == null ? default(T) : (T)token;
        }

        public virtual IEnumerable<VToken> Children()
        {
            return Enumerable.Empty<VToken>();
        }

        public IEnumerable<T> Children<T>() where T : VToken
        {
            return Children().OfType<T>();
        }

        protected abstract bool DeepEquals(VToken node);

        public override string ToString()
        {
            using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                KVTextWriter vdfTextWriter = new KVTextWriter(stringWriter);
                WriteTo(vdfTextWriter);

                return stringWriter.ToString();
            }
        }
    }
}