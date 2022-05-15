using System;

namespace uSource.Formats.Utilities
{
    internal static class MiscellaneousUtils
    {
        public static string ObjectToString(object value)
        {
            if (value == null)
            {
                return "{null}";
            }

            return (value is string) ? string.Format(@"""{0}""", value) : value.ToString();
        }

        public static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }
    }
}