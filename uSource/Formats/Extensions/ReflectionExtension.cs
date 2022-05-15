using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace uSource.Formats.Extensions
{
    /// <summary>
    /// Extension reflections
    /// </summary>
    public static class ReflectionExtension
    {
        static HashSet<Type> SimpleTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        };
        public static bool IsSimpleType(this Type t)
        {
            t = (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) ? t.GetGenericArguments()[0] : t;
            if (t.IsPrimitive)
                return true;

            return SimpleTypes.Contains(t) || t.IsEnum || Convert.GetTypeCode(t) != TypeCode.Object;
        }

        public static bool IsPrimitive(this Type t)
        {
            return (t.IsSimpleType() && t.IsValueType) || SimpleTypes.Contains(t) || t.IsEnum || Convert.GetTypeCode(t) != TypeCode.Object || IsUnityObject(t);
        }

        public static bool IsUnityObject(this Type t)
        {
#if UNITY_EDITOR
            return t.IsSubclassOf(typeof(UnityEngine.Object));
#else
            return false;
#endif
        }
    }
}