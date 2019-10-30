using System;
using System.Runtime.Serialization;

namespace SaveStorageSettings {
    public class BadHeaderException : Exception {
        public BadHeaderException() {}
        public BadHeaderException(string message) : base(message) {}
        public BadHeaderException(string message, Exception innerException) : base(message, innerException) {}
        protected BadHeaderException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}
