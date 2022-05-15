using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace uSource.Formats.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class NotSerializeAttribute : Attribute
    {
    }

    public class Serialization
    {
        public static bool IsValidMember(MemberInfo member, out FieldInfo field, out PropertyInfo prop, out Type type)
        {
            field = member as FieldInfo;
            prop = member as PropertyInfo;

            if ((field == null || field.IsDefined(typeof(NotSerializeAttribute), true)) && (prop == null || (prop.CanRead && !prop.CanWrite) || prop.GetIndexParameters().Length > 0 || prop.IsDefined(typeof(NotSerializeAttribute), true)))
            {
                type = null;
                return false;
            }

            type = field != null ? field.FieldType : prop.PropertyType;
            return true;
        }
    }
}