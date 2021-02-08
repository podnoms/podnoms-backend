using System;

namespace PodNoms.AudioParsing.ErrorHandling {
    public class AudioDownloadException : Exception {
        public AudioDownloadException(string message) : base(message) {
        }
    }
}
