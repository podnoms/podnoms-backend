using System;

namespace PodNoms.Common.Auth {
    public class NotAuthorisedException : Exception {
        public NotAuthorisedException(string message) : base(message) {
        }
    }
}
