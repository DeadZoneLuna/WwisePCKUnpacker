using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using uSource.Formats.Extensions;
using uSource.Formats.Utilities;

namespace uSource.Formats.Serialization.KV1
{
    public class KVSerializer
    {
        private readonly KVSerializerSettings _settings;

        public KVSerializer() : this(KVSerializerSettings.Default) { }

        public KVSerializer(KVSerializerSettings settings)
        {
            _settings = settings;
        }

        public static string Serialize(object obj, bool clearHashes = true)
        {
            MiscellaneousUtils.CheckArgumentNull(obj, "obj");

            using (var stringWriter = new StringWriter(KVStructure.Culture))
            {
                using (var vdfWriter = new KVTextWriter(stringWriter))
                    KVSerialize.WriteTable(vdfWriter, obj.ConvertObjectTypeToString(), obj, obj.GetType() is IEnumerable ? false : true);

                return stringWriter.ToString();
            }
        }

        public void Serialize(TextWriter textWriter, VToken value)
        {
            using (KVWriter vdfWriter = new KVTextWriter(textWriter, _settings))
                value.WriteTo(vdfWriter);
        }

        public VProperty Deserialize(TextReader textReader)
        {
            using (KVReader vdfReader = new KVTextReader(textReader, _settings))
            {
                if (_settings.IgnoreToken) vdfReader.CurrentState = KVReader.State.Object;
                else if (!vdfReader.ReadToken())
                    throw new KVException("Incomplete VDF data.");

                // For now, we discard these comments.
                while (vdfReader.CurrentState == KVReader.State.Comment)
                    if (!vdfReader.ReadToken())
                        throw new KVException("Incomplete VDF data.");

                return _settings.IgnoreToken ? new VProperty(string.Empty, ReadObject(vdfReader)) : ReadProperty(vdfReader);
            }
        }

        private VProperty ReadProperty(KVReader reader)
        {
            // Setting it to null is temporary, we'll set Value in just a second.
            VProperty result = new VProperty(reader.Value == null ? string.Empty : reader.Value, null);

            if (!(_settings.IgnoreToken && reader.CurrentState == KVReader.State.Object) && !reader.ReadToken())
                throw new KVException("Incomplete VDF data.");

            // For now, we discard these comments.
            while (reader.CurrentState == KVReader.State.Comment)
                if (!reader.ReadToken())
                    throw new KVException("Incomplete VDF data.");

            if (reader.CurrentState == KVReader.State.Property) result.Value = new VValue(reader.Value);
            else result.Value = ReadObject(reader);

            return result;
        }

        private VObject ReadObject(KVReader reader)
        {
            VObject result = new VObject();

            if (!reader.ReadToken())
                throw new KVException("Incomplete VDF data.");

            while (!(reader.CurrentState == KVReader.State.Object && reader.Value == KVStructure.ObjectEnd.ToString()))
            {
                if (reader.CurrentState == KVReader.State.Comment)
                    result.Add(VValue.CreateComment(reader.Value));
                else
                    result.Add(ReadProperty(reader));

                if (!reader.ReadToken())
                    return result;//throw new KVException("Incomplete VDF data.");
            }

            return result;
        }
    }
}