using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace uSource.Formats.Serialization.KV1
{
    public static class KVConvert
    {
        public static string Serialize(VToken value)
        {
            return Serialize(value, KVSerializerSettings.Common);
        }

        public static string Serialize(VToken value, KVSerializerSettings settings)
        {
            if (value == null)
                throw new ArgumentNullException("value");//nameof(value));

            StringBuilder stringBuilder = new StringBuilder(256);
            StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
            (new KVSerializer(settings)).Serialize(stringWriter, value);

            return stringWriter.ToString();
        }

        public static VProperty Deserialize(string value)
        {
            return Deserialize(value, KVSerializerSettings.Common);
        }

        public static VProperty Deserialize(string value, KVSerializerSettings settings)
        {
            return Deserialize(new StringReader(value), settings);
        }

        public static VProperty Deserialize(Stream stream, KVSerializerSettings settings)
        {
            using (var reader = new StreamReader(stream))
                return Deserialize(reader, settings);
        }

        public static VProperty Deserialize(TextReader reader)
        {
            return Deserialize(reader, KVSerializerSettings.Common);
        }

        public static VProperty Deserialize(TextReader reader, KVSerializerSettings settings)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");//nameof(reader));

            return new KVSerializer(settings).Deserialize(reader);
        }
    }
}