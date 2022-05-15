using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using uSource.Formats.Utilities;

namespace uSource.Formats.Serialization.KV1
{
    public class VObject : VToken, IList<VToken>, IDictionary<string, VToken>
    {
        private readonly List<VToken> _children;

        public VObject()
        {
            _children = new List<VToken>();
        }

        public VObject(VObject other)
        {
            _children = other._children.Select(x => x.DeepClone()).ToList();
        }

        public override VTokenType Type
        {
            get
            {
                return VTokenType.Object;
            }
        }

        public int Count
        {
            get
            {
                return _children.Count;
            }
        }

        public override VToken this[object key]
        {
            get
            {
                MiscellaneousUtils.CheckArgumentNull(key, "key");//nameof(key));

                string propertyName;
                if ((propertyName = key as string) == null)
                    throw new ArgumentException(string.Format("Accessed VObject values with invalid key value: {0}. Object property name expected.", MiscellaneousUtils.ObjectToString(key)));

                return this[propertyName];
            }

            set
            {
                MiscellaneousUtils.CheckArgumentNull(key, "key");//nameof(key));

                string propertyName;
                if ((propertyName = key as string) == null)
                    throw new ArgumentException(string.Format("Set VObject values with invalid key value: {0}. Object property name expected.", MiscellaneousUtils.ObjectToString(key)));

                this[propertyName] = value;
            }
        }

        public VToken this[int index]
        {
            get
            {
                return _children[index];
            }
            set
            {
                _children[index] = value;
            }
        }

        public VToken this[string key]
        {
            get
            {
                VToken result;
                if (!TryGetValue(key, out result))
                    return null;

                return result;
            }

            set
            {
                VProperty prop = Properties().FirstOrDefault(x => x.Key == key);
                VToken token = value == null ? VValue.CreateEmpty() : value;

                if (prop != null) prop.Value = token;
                else Add(key, token);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        ICollection<string> IDictionary<string, VToken>.Keys
        {
            get
            {
                return Properties().Select(x => x.Key).ToList();
            }
        }

        ICollection<VToken> IDictionary<string, VToken>.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<VToken> Children()
        {
            return _children;
        }

        public IEnumerable<VProperty> Properties()
        {
            return _children.Where(x => x is VProperty).OfType<VProperty>();
        }

        public void Add(string key, VToken value)
        {
            Add(new VProperty(key, value));
        }

        public void Add(VProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");//nameof(property));
            if (property.Value == null)
                throw new ArgumentNullException("property.Value");//nameof(property.Value));

            _children.Add(property);
        }

        public void Add(VToken token)
        {
            if (token == null)
                throw new ArgumentNullException("token");//nameof(token));

            _children.Add(token);
        }

        public void Clear()
        {
            _children.Clear();
        }

        public bool Contains(VToken item)
        {
            return _children.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return Properties().Any(x => x.Key == key);
        }

        public void CopyTo(VToken[] array, int arrayIndex)
        {
            _children.CopyTo(array, arrayIndex);
        }

        public override VToken DeepClone()
        {
            return new VObject(this);
        }

        public int IndexOf(VToken item)
        {
            return _children.IndexOf(item);
        }

        public void Insert(int index, VToken item)
        {
            _children.Insert(index, item);
        }

        public bool Remove(string key)
        {
            VProperty p;
            return _children.RemoveAll(x => (p = x as VProperty) != null && p.Key == key) != 0;
        }

        public bool Remove(VToken item)
        {
            return _children.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _children.RemoveAt(index);
        }

        public bool TryGetValue(string key, out VToken value)
        {
            VProperty vProperty = Properties().FirstOrDefault(x => x.Key == key);
            value = vProperty != null ? vProperty.Value : null;
            return value != null;
        }

        public override void WriteTo(KVWriter writer)
        {
            writer.WriteObjectStart();

            foreach (VToken child in _children)
                child.WriteTo(writer);

            writer.WriteObjectEnd();
        }

        public void MergeFrom(VObject from, bool replace = false)
        {
            if (from == null || from.Count == 0) return;

            foreach (var item in from)
            {
                if (ContainsKey(item.Key) && replace) this[item.Key] = item.Value;
                else Add(item.Key, item.Value);
            }
        }

        #region ICollection<KeyValuePair<string,JToken>> Members

        public IEnumerator<KeyValuePair<string, VToken>> GetEnumerator()
        {
            foreach (VProperty property in Properties())
                yield return new KeyValuePair<string, VToken>(property.Key, property.Value);
        }

        VToken IDictionary<string, VToken>.this[string key]
        {
            get
            {
                VToken value = this[key];
                if (value != null) return value;

                throw new KeyNotFoundException();
            }

            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this[key] = value;
            }
        }

        void ICollection<KeyValuePair<string, VToken>>.Add(KeyValuePair<string, VToken> item)
        {
            Add(new VProperty(item.Key, item.Value));
        }

        void ICollection<KeyValuePair<string, VToken>>.Clear()
        {
            _children.Clear();
        }

        bool ICollection<KeyValuePair<string, VToken>>.Contains(KeyValuePair<string, VToken> item)
        {
            VProperty property = Properties().FirstOrDefault(x => x.Key == item.Key);
            if (property == null)
                return false;

            return (property.Value == item.Value);
        }

        void ICollection<KeyValuePair<string, VToken>>.CopyTo(KeyValuePair<string, VToken>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");//nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
            if (arrayIndex >= array.Length && arrayIndex != 0)
                throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("The number of elements in the source VObject is greater than the available space from arrayIndex to the end of the destination array.");

            int index = 0;
            foreach (VProperty property in Properties())
                array[arrayIndex + index++] = new KeyValuePair<string, VToken>(property.Key, property.Value);
        }

        bool ICollection<KeyValuePair<string, VToken>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<KeyValuePair<string, VToken>>.Remove(KeyValuePair<string, VToken> item)
        {
            if (!((ICollection<KeyValuePair<string, VToken>>)this).Contains(item))
                return false;

            ((IDictionary<string, VToken>)this).Remove(item.Key);
            return true;
        }

        #endregion

        protected override bool DeepEquals(VToken token)
        {
            VObject otherObj;
            if ((otherObj = token as VObject) == null)
                return false;

            return _children.Count == otherObj._children.Count && Enumerable.Range(0, _children.Count).All(x => DeepEquals(_children[x], otherObj._children[x]));
        }
    }
}