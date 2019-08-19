using System;

namespace PodNoms.Common.Services.Downloader {
    public class AudioDownloadException : Exception {
        public AudioDownloadException(string message) : base(message) {
        }
    }
}
