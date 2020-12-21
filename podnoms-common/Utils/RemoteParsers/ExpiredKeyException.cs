using System;

namespace PodNoms.Common.Utils.RemoteParsers {
    public class ExpiredKeyException : Exception {
        public ExpiredKeyException(string message) : base(message) {
        }
    }
    public class NoKeyAvailableException : Exception {
    }
}
