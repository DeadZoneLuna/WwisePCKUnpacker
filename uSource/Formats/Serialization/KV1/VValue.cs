using System;
using System.Globalization;
using System.Text.RegularExpressions;
using uSource.Formats.Extensions;

namespace uSource.Formats.Serialization.KV1
{
    public class VValue : VToken
    {
        private readonly VTokenType _tokenType;
        public string Value { get; set; }

        public VValue(string value, VTokenType type)
        {
            Value = value;
            _tokenType = type;
        }

        public VValue(string value) : this(value, VTokenType.Value) { }

        public VValue(VValue other) : this(other.Value, other.Type) { }

        public override VTokenType Type
        {
            get
            {
                return _tokenType;
            }
        }

        public override VToken DeepClone()
        {
            return new VValue(this);
        }

        public override void WriteTo(KVWriter writer)
        {
            if (_tokenType == VTokenType.Comment) writer.WriteComment(ToString());
            else writer.WriteValue(this);
        }

        public override string ToString()
        {
            return Value == null ? string.Empty : Value;
        }

        public static VValue CreateComment(string value)
        {
            return new VValue(value, VTokenType.Comment);
        }

        public static VValue CreateEmpty()
        {
            return new VValue(string.Empty);
        }

        protected override bool DeepEquals(VToken token)
        {
            VValue otherVal;
            if ((otherVal = token as VValue) == null)
                return false;

            return (this == otherVal || (Type == otherVal.Type && Value != null && Value.Equals(otherVal.Value)));
        }

        public static bool operator ==(VValue a, object b)
        {
            return ReferenceEquals(a, b) || b == null && a.Value == null;//a.IsNull;
        }

        public static bool operator !=(VValue a, object b)
        {
            return !(a == b);
        }

        public static implicit operator string(VValue entry)
        {
            return entry.ToString();
        }

        public static implicit operator byte(VValue entry)
        {
            return entry.Value.ConvertToByte();
        }

        public static implicit operator sbyte(VValue entry)
        {
            return entry.Value.ConvertToSByte();
        }

        public static implicit operator char(VValue entry)
        {
            return entry.Value.ConvertToChar();
        }

        public static implicit operator bool(VValue entry)
        {
            return (int)entry != 0;
        }

        #region Int
        public static implicit operator short(VValue entry)
        {
            return entry.Value.ConvertToInt16();
        }

        public static implicit operator ushort(VValue entry)
        {
            return entry.Value.ConvertToUInt16();
        }

        public static implicit operator int(VValue entry)
        {
            return entry.Value.ConvertToInt32();
        }

        public static implicit operator uint(VValue entry)
        {
            return entry.Value.ConvertToUInt32();
        }

        public static implicit operator long(VValue entry)
        {
            return entry.Value.ConvertToInt64();
        }

        public static implicit operator ulong(VValue entry)
        {
            return entry.Value.ConvertToUInt64();
        }

        #endregion

        #region Float
        public static implicit operator float(VValue entry)
        {
            return entry.Value.ConvertToSingle();
        }

        public static implicit operator double(VValue entry)
        {
            return entry.Value.ConvertToDouble();
        }

        public static implicit operator decimal(VValue entry)
        {
            return entry.Value.ConvertToDecimal();
        }
        #endregion

        public static implicit operator DateTime(VValue entry)
        {
            return entry.Value.ConvertToDateTime();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}