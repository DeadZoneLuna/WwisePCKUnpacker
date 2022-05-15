using System;

namespace uSource.Formats.Serialization.KV1
{
    public class VProperty : VToken
    {
        // Json.NET calls this 'Name', but since VDF is technically KeyValues we call it a 'Key'.
        public string Key { get; set; }
        public VToken Value { get; set; }

        public VProperty()
        {
            Key = string.Empty;
            Value = new VValue(string.Empty);
        }

        public VProperty(string key, VToken value)
        {
            if (key == null)
                throw new ArgumentNullException("key");//nameof(key));

            Key = key;
            Value = value;
        }

        public VProperty(VProperty other)
            : this(other.Key, other.Value.DeepClone()) { }

        public override VTokenType Type
        {
            get
            {
                return VTokenType.Property;
            }
        }

        public override VToken DeepClone()
        {
            return new VProperty(this);
        }

        public override void WriteTo(KVWriter writer)
        {
            writer.WriteKey(Key);
            Value.WriteTo(writer);
        }

        protected override bool DeepEquals(VToken node)
        {
            VProperty otherProp;
            return (otherProp = node as VProperty) != null && Key == otherProp.Key && VToken.DeepEquals(Value, otherProp.Value);
        }
    }
}