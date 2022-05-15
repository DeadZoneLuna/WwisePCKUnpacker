using System;
using System.IO;

namespace uSource.Formats.Serialization.KV1
{
    public class KVTextWriter : KVWriter
    {
        private readonly TextWriter _writer;
        private int _indentationLevel;

        public KVTextWriter(TextWriter writer) : this(writer, KVSerializerSettings.Default) { }

        public KVTextWriter(TextWriter writer, KVSerializerSettings settings) : base(settings)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");//nameof(writer));

            _writer = writer;
            _indentationLevel = 0;
        }

        public override void WriteKey(string key)
        {
            AutoComplete(State.Key);
            _writer.Write(KVStructure.Quote);
            WriteEscapedString(key);
            _writer.Write(KVStructure.Quote);
        }

        public override void WriteValue(VValue value)
        {
            AutoComplete(State.Value);
            _writer.Write(KVStructure.Quote);
            WriteEscapedString(value.ToString());
            _writer.Write(KVStructure.Quote);
        }

        public override void WriteObjectStart()
        {
            AutoComplete(State.ObjectStart);
            _writer.Write(KVStructure.ObjectStart);

            _indentationLevel++;
        }

        public override void WriteObjectEnd()
        {
            _indentationLevel--;

            AutoComplete(State.ObjectEnd);
            _writer.Write(KVStructure.ObjectEnd);

            if (_indentationLevel == 0)
                AutoComplete(State.Finished);
        }

        public override void WriteComment(string text)
        {
            AutoComplete(State.Comment);
            _writer.Write(KVStructure.Comment);
            _writer.Write(KVStructure.Comment);
            _writer.Write(text);
        }

        private void AutoComplete(State next)
        {
            if (CurrentState == State.Start)
            {
                CurrentState = next;
                return;
            }

            switch (next)
            {
                case State.Value:
                    _writer.Write(KVStructure.Assign);
                    break;

                case State.Key:
                case State.ObjectStart:
                case State.ObjectEnd:
                case State.Comment:
                    _writer.WriteLine();
                    _writer.Write(new string(KVStructure.Indent, _indentationLevel));
                    break;

                case State.Finished:
                    _writer.WriteLine();
                    break;
            }

            CurrentState = next;
        }

        private void WriteEscapedString(string str)
        {
            if (!Settings.UsesEscapeSequences)
            {
                _writer.Write(str);
                return;
            }

            foreach (char ch in str)
            {
                if (!KVStructure.IsEscapable(ch))
                    _writer.Write(ch);
                else
                {
                    _writer.Write(KVStructure.Escape);
                    _writer.Write(KVStructure.GetEscape(ch));
                }
            }
        }

        public override void Close()
        {
            base.Close();
            if (CloseOutput)
                _writer.Dispose();
        }
    }
}