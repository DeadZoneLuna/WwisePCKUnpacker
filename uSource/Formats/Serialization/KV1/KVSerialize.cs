using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using uSource.Formats.Serialization;
using uSource.Formats.Extensions;

namespace uSource.Formats.Serialization.KV1
{
    /// <summary>
    /// Purpose:
    /// <br>Internal serialize .net objects into KeyValue format</br>
    /// </summary>
    public class KVSerialize
    {
        internal static void SerializeMultidimensionalArray(KVTextWriter kvwriter, Array array, int[] indices = null, int maxDimensions = 2, int lastDimension = -1)
        {
            int dimension = ++lastDimension;
            if (dimension == 0)
            {
                #region Rank Size
                maxDimensions = array.Rank - 1;
                indices = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                {
                    indices[i] = array.GetLength(i);
                }

                SerializeValue(kvwriter, indices);
                #endregion

                kvwriter.WriteKey(KVStructure.RankRootKey);
                kvwriter.WriteObjectStart();
            }

            for (int i = 0; i < array.GetLength(dimension); i++)
            {
                indices[dimension] = i;
                if (dimension != maxDimensions)
                {
                    kvwriter.WriteKey(KVStructure.RankKey);
                    kvwriter.WriteObjectStart();
                    SerializeMultidimensionalArray(kvwriter, array, indices, maxDimensions, dimension);
                    kvwriter.WriteObjectEnd();
                }
                else SerializeValue(kvwriter, array.GetValue(indices));
            }

            if (dimension == 0) kvwriter.WriteObjectEnd();
        }

        internal static void SerializeValue(KVTextWriter kvwriter, object obj, bool skipTableWrite = false)
        {
            if (obj != null)
            {
                Type objType = obj.GetType();

                if (objType.IsPrimitive())
                {
                    WriteTable(kvwriter, obj, null);
                    return;
                }

                if (!skipTableWrite)
                {
                    kvwriter.WriteKey(objType.GetTypeName());
                    kvwriter.WriteObjectStart();
                }

                VToken tempToken = obj as VToken;
                if (tempToken != null)
                {
                    if (tempToken.Type == VTokenType.Property) WriteProperty(kvwriter, (VProperty)obj);
                    if (tempToken.Type == VTokenType.Object) WriteObject(kvwriter, (VObject)obj, false);
                }
                else
                {
                    if (!SerializeEnumerable(kvwriter, obj, objType))
                    {
                        foreach (MemberInfo member in objType.GetMembers(KVStructure.Bindings))
                        {
                            FieldInfo field;
                            PropertyInfo prop;
                            Type type;
                            if (!Serialization.IsValidMember(member, out field, out prop, out type))
                                continue;

                            object value = field != null ? field.GetValue(obj) : prop.GetValue(obj, null);

                            if (type.IsPrimitive()) WriteKV(kvwriter, member.Name, value);
                            else WriteTable(kvwriter, value.ConvertObjectTypeToString(type), value, true);
                        }
                    }
                }

                if (!skipTableWrite) kvwriter.WriteObjectEnd();
            }
        }

        internal static bool SerializeEnumerable(KVTextWriter kvwriter, object obj, Type objType)
        {
            if (!objType.IsValueType)
            {
                IEnumerable enumerableObj = obj as IEnumerable;
                if (enumerableObj != null)
                {
                    IDictionary dict = obj as IDictionary;
                    if (dict != null)
                        serializeDictMethod.MakeGenericMethod(objType.GetGenericArguments()).Invoke(null, new object[] { kvwriter, obj });
                    else
                    {
                        if (objType.IsArray && objType.GetArrayRank() > 1) SerializeMultidimensionalArray(kvwriter, obj as Array);
                        else
                        {
                            foreach (object child in enumerableObj)
                            {
                                SerializeValue(kvwriter, child);
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        static MethodInfo serializeDictMethod = typeof(KVSerialize).GetMethod("SerializeDictionary");
        public static void SerializeDictionary<TKey, TValue>(KVTextWriter kvwriter, IDictionary<TKey, TValue> dict)
        {
            Type[] genericArgs = dict.GetType().GetGenericArguments();
            bool keyIsPrimitive = genericArgs[0].IsPrimitive();
            //TODO: bool valueIsPrimitive = genericArgs[1].IsPrimitive();
            foreach (KeyValuePair<TKey, TValue> pair in dict)
            {
                WriteKey(kvwriter, pair.Key, keyIsPrimitive);
                kvwriter.WriteObjectStart();
                {
                    if (!keyIsPrimitive) SerializeValue(kvwriter, pair.Key, true);
                    SerializeValue(kvwriter, pair.Value);
                }
                kvwriter.WriteObjectEnd();
            }
        }

        #region Writers
        internal static void WriteProperty(KVTextWriter kvwriter, VProperty vprop)
        {
            if (vprop.Value.Type != VTokenType.Object)
            {
                kvwriter.WriteKey(vprop.Key);
                switch (vprop.Value.Type)
                {
                    case VTokenType.Property: WriteProperty(kvwriter, (VProperty)vprop.Value); break;
                    case VTokenType.Object: WriteObject(kvwriter, (VObject)vprop.Value); break;
                    case VTokenType.Value: kvwriter.WriteValue((VValue)vprop.Value); break;
                }
            }
        }

        internal static void WriteObject(KVTextWriter kvwriter, VObject vobject, bool writeTable = true)
        {
            if (writeTable) kvwriter.WriteObjectStart();
            foreach (VToken child in vobject.Children())
            {
                switch (child.Type)
                {
                    case VTokenType.Property:
                        WriteProperty(kvwriter, (VProperty)child);
                        break;

                    default:
                        child.WriteTo(kvwriter);
                        break;
                }
            }
            if (writeTable) kvwriter.WriteObjectEnd();
        }

        internal static void WriteTable(KVTextWriter kvwriter, object key, object value, bool skip = false)
        {
            WriteKey(kvwriter, key);
            kvwriter.WriteObjectStart();
            if (value != null) SerializeValue(kvwriter, value, skip);
            kvwriter.WriteObjectEnd();
        }

        internal static void WriteKey(KVTextWriter kvwriter, object key, bool isPrimitive = true)
        {
            kvwriter.WriteKey(isPrimitive ? key.ConvertToString() : key.ConvertObjectTypeToString());
        }

        internal static void WriteValue(KVTextWriter kvwriter, object value, bool isPrimitive = true)
        {
            kvwriter.WriteValue(new VValue(isPrimitive ? value.ConvertToString() : value.ConvertObjectTypeToString()));
        }

        internal static void WriteKV(KVTextWriter kvwriter, object key, object value)
        {
            WriteKey(kvwriter, key);
            WriteValue(kvwriter, value);
        }
        #endregion
    }
}