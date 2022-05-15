using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uSource.Formats.Extensions
{
    public static class TypeConverterExtension
    {
        public static Regex FloatRegex = new Regex(@"[-]?([0-9]+([.,][0-9]*)?|[.][0-9]+)");
        public static CultureInfo Culture = CultureInfo.InvariantCulture;
        public static readonly string True = "1";
        public static readonly string False = "0";
        public static readonly string Null = "Null";
        public static readonly string PositiveInfinity = "Infinity";
        public static readonly string NegativeInfinity = "-Infinity";
        public static readonly string NaN = "NaN";

        #region Type -> String
        static string ConvertFloatToString(this object value)
        {
            float _value = (float)value;
            if (float.IsNaN(_value)) return NaN;
            if (float.IsPositiveInfinity(_value)) return PositiveInfinity;
            if (float.IsNegativeInfinity(_value)) return NegativeInfinity;
            return _value.ToString("G9", Culture);
        }

        static string ConvertDoubleToString(this object value)
        {
            double _value = (double)value;
            if (double.IsNaN(_value)) return NaN;
            if (double.IsPositiveInfinity(_value)) return PositiveInfinity;
            if (double.IsNegativeInfinity(_value)) return NegativeInfinity;
            return _value.ToString("G17", Culture);
        }

        public static string ConvertObjectTypeToString(this object value, Type subType = null)
        {
            if (value != null) return value.GetType().GetTypeName();
            if (subType != null) return subType.GetTypeName();

            return string.Empty;
        }

        public static string ConvertToString(this object value)
        {
            if (value == null)
                return Null;

            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    bool _value = (bool)value;
                    if (_value) return True;
                    else return False;
                case TypeCode.Single: return value.ConvertFloatToString();
                case TypeCode.Double: return value.ConvertDoubleToString();
                case TypeCode.Decimal: return Convert.ToString(value, Culture);
                case TypeCode.DateTime: return Convert.ToString(value, Culture);
                default:
                    if (value is Enum) return Convert.ChangeType(value, typeCode).ToString();
#if UNITY_EDITOR
                    if (value is UnityEngine.Object) return ObjectExtension.GetAssetPathFromObject(value as UnityEngine.Object);
#endif
                    return value.ToString();
            }
        }
        #endregion

        #region String -> Type
        public static string GetTypeName(this Type type)
        {
            return type.FullName;//.ToString();
        }

        public static object ConvertToType(this string input, Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean: return input.ConvertToBoolean();
                case TypeCode.Char: return input.ConvertToChar();
                case TypeCode.SByte: return input.ConvertToSByte();
                case TypeCode.Byte: return input.ConvertToByte();
                case TypeCode.Int16: return input.ConvertToInt16();
                case TypeCode.UInt16: return input.ConvertToUInt16();
                case TypeCode.Int32: return input.ConvertToInt32();
                case TypeCode.UInt32: return input.ConvertToUInt32();
                case TypeCode.Int64: return input.ConvertToInt64();
                case TypeCode.UInt64: return input.ConvertToUInt64();
                case TypeCode.Single: return input.ConvertToSingle();
                case TypeCode.Double: return input.ConvertToDouble();
                case TypeCode.Decimal: return input.ConvertToDecimal();
                case TypeCode.DateTime: return input.ConvertToDateTime();
                default:
                    if (type.IsEnum) return input.ConvertToEnum(type);
#if UNITY_EDITOR
                    if (type.IsUnityObject()) return AssetDatabase.LoadAssetAtPath(input, type);
#endif
                    if (input != Null) return input;

                    return Nullable.GetUnderlyingType(type);
            }
        }

        public static byte ConvertToByte(this string input)
        {
            byte result;
            return byte.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : (byte)0;
        }

        public static sbyte ConvertToSByte(this string input)
        {
            sbyte result;
            return sbyte.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : (sbyte)0;
        }

        public static short ConvertToInt16(this string input)
        {
            short result;
            return short.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : (short)0;
        }

        public static ushort ConvertToUInt16(this string input)
        {
            ushort result;
            return ushort.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : (ushort)0;
        }

        public static int ConvertToInt32(this string input)
        {
            int result;
            return int.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : 0;
        }

        public static uint ConvertToUInt32(this string input)
        {
            uint result;
            return uint.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : 0U;
        }

        public static long ConvertToInt64(this string input)
        {
            long result;
            return long.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : 0L;
        }

        public static ulong ConvertToUInt64(this string input)
        {
            ulong result;
            return ulong.TryParse(input, NumberStyles.Integer, Culture, out result) ? result : 0UL;
        }

        public static float ConvertToSingle(this string input)
        {
            float result;
            return float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, Culture, out result) ? result : 0F;
        }

        public static double ConvertToDouble(this string input)
        {
            double result;
            return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, Culture, out result) ? result : 0D;
        }

        public static decimal ConvertToDecimal(this string input)
        {
            decimal result;
            return DecimalTryParse(input.ToCharArray(), 0, input.Length, out result) ? result : 0M;
        }

        public static char ConvertToChar(this string input)
        {
            char result;
            return char.TryParse(input, out result) ? result : ' ';
        }

        public static bool ConvertToBoolean(this string input)
        {
            return input.ConvertToInt32() != 0;
        }

        public static DateTime ConvertToDateTime(this string input)
        {
            DateTime result;
            return DateTime.TryParse(input, out result) ? result : new DateTime();
        }

        public static object ConvertToEnum(this string input, Type type)
        {
            return Enum.Parse(type, input);
        }

        #region Parse
        public static bool DecimalTryParse(char[] chars, int start, int length, out decimal value)
        {
            value = 0M;
            const decimal decimalMaxValueHi28 = 7922816251426433759354395033M;
            const ulong decimalMaxValueHi19 = 7922816251426433759UL;
            const ulong decimalMaxValueLo9 = 354395033UL;
            const char decimalMaxValueLo1 = '5';

            if (length == 0)
            {
                return false;
            }

            bool isNegative = (chars[start] == '-');
            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return false;
                }

                start++;
                length--;
            }

            int i = start;
            int end = start + length;
            int numDecimalStart = end;
            int numDecimalEnd = end;
            int exponent = 0;
            ulong hi19 = 0UL;
            ulong lo10 = 0UL;
            int mantissaDigits = 0;
            int exponentFromMantissa = 0;
            char? digit29 = null;
            bool? storeOnly28Digits = null;
            for (; i < end; i++)
            {
                char c = chars[i];
                switch (c)
                {
                    case '.':
                        if (i == start)
                        {
                            return false;
                        }
                        if (i + 1 == end)
                        {
                            return false;
                        }

                        if (numDecimalStart != end)
                        {
                            // multiple decimal points
                            return false;
                        }

                        numDecimalStart = i + 1;
                        break;
                    case 'e':
                    case 'E':
                        if (i == start)
                        {
                            return false;
                        }
                        if (i == numDecimalStart)
                        {
                            // E follows decimal point		
                            return false;
                        }
                        i++;
                        if (i == end)
                        {
                            return false;
                        }

                        if (numDecimalStart < end)
                        {
                            numDecimalEnd = i - 1;
                        }

                        c = chars[i];
                        bool exponentNegative = false;
                        switch (c)
                        {
                            case '-':
                                exponentNegative = true;
                                i++;
                                break;
                            case '+':
                                i++;
                                break;
                        }

                        // parse 3 digit 
                        for (; i < end; i++)
                        {
                            c = chars[i];
                            if (c < '0' || c > '9')
                            {
                                return false;
                            }

                            int newExponent = (10 * exponent) + (c - '0');
                            // stops updating exponent when overflowing
                            if (exponent < newExponent)
                            {
                                exponent = newExponent;
                            }
                        }

                        if (exponentNegative)
                        {
                            exponent = -exponent;
                        }
                        break;
                    default:
                        if (c < '0' || c > '9')
                        {
                            return false;
                        }

                        if (i == start && c == '0')
                        {
                            i++;
                            if (i != end)
                            {
                                c = chars[i];
                                if (c == '.')
                                {
                                    goto case '.';
                                }
                                if (c == 'e' || c == 'E')
                                {
                                    goto case 'E';
                                }

                                return false;
                            }
                        }

                        if (mantissaDigits < 29 && (mantissaDigits != 28 || !(storeOnly28Digits ?? (storeOnly28Digits = (hi19 > decimalMaxValueHi19 || (hi19 == decimalMaxValueHi19 && (lo10 > decimalMaxValueLo9 || (lo10 == decimalMaxValueLo9 && c > decimalMaxValueLo1))))).GetValueOrDefault())))
                        {
                            if (mantissaDigits < 19)
                            {
                                hi19 = (hi19 * 10UL) + (ulong)(c - '0');
                            }
                            else
                            {
                                lo10 = (lo10 * 10UL) + (ulong)(c - '0');
                            }
                            ++mantissaDigits;
                        }
                        else
                        {
                            if (!digit29.HasValue)
                            {
                                digit29 = c;
                            }
                            ++exponentFromMantissa;
                        }
                        break;
                }
            }

            exponent += exponentFromMantissa;

            // correct the decimal point
            exponent -= (numDecimalEnd - numDecimalStart);

            if (mantissaDigits <= 19)
            {
                value = hi19;
            }
            else
            {
                value = (hi19 / new decimal(1, 0, 0, false, (byte)(mantissaDigits - 19))) + lo10;
            }

            if (exponent > 0)
            {
                mantissaDigits += exponent;
                if (mantissaDigits > 29)
                {
                    return false;
                }
                if (mantissaDigits == 29)
                {
                    if (exponent > 1)
                    {
                        value /= new decimal(1, 0, 0, false, (byte)(exponent - 1));
                        if (value > decimalMaxValueHi28)
                        {
                            return false;
                        }
                    }
                    else if (value == decimalMaxValueHi28 && digit29 > decimalMaxValueLo1)
                    {
                        return false;
                    }
                    value *= 10M;
                }
                else
                {
                    value /= new decimal(1, 0, 0, false, (byte)exponent);
                }
            }
            else
            {
                if (digit29 >= '5' && exponent >= -28)
                {
                    ++value;
                }
                if (exponent < 0)
                {
                    if (mantissaDigits + exponent + 28 <= 0)
                    {
                        value = isNegative ? -0M : 0M;
                        return true;
                    }
                    if (exponent >= -28)
                    {
                        value *= new decimal(1, 0, 0, false, (byte)(-exponent));
                    }
                    else
                    {
                        value /= 1e28M;
                        value *= new decimal(1, 0, 0, false, (byte)(-exponent - 28));
                    }
                }
            }

            if (isNegative)
            {
                value = -value;
            }

            return true;
        }

        public static bool TryDoubleParse(this string input, out double result)
        {
            string sMaxValue = "1.7976931348623157E+308";
            string sMinValue = "-1.7976931348623157E+308";

            if (input == sMaxValue) { result = double.MaxValue; return true; }
            else if (input == sMinValue) { result = double.MinValue; return true; }

            return double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }
        #endregion
        #endregion
    }
}