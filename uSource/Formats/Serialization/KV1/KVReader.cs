using System;

namespace uSource.Formats.Serialization.KV1
{
    public abstract class KVReader : IDisposable
    {
        public KVSerializerSettings Settings { get; }
        public bool CloseInput { get; set; }
        public string Value { get; set; }

        protected internal State CurrentState { get; set; }

        protected KVReader() : this(KVSerializerSettings.Default) { }

        protected KVReader(KVSerializerSettings settings)
        {
            Settings = settings;

            CurrentState = State.Start;
            Value = null;
            CloseInput = true;
        }

        public abstract bool ReadToken();

        void IDisposable.Dispose()
        {
            if (CurrentState == State.Closed)
                return;

            Close();
        }

        public virtual void Close()
        {
            CurrentState = State.Closed;
            Value = null;
        }

        protected internal enum State
        {
            Start,
            Property,
            Object,
            Comment,
            Conditional,
            Finished,
            Closed
        }
    }
}