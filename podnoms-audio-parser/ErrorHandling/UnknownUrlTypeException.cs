using System;

namespace PodNoms.AudioParsing.ErrorHandling {
    internal class UnknownUrlTypeException : Exception {
        public UnknownUrlTypeException(string message) : base(message) {
        }
    }
}
