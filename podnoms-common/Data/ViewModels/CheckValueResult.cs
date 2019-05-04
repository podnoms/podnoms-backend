namespace PodNoms.Common.Data.ViewModels {
    public sealed class CheckValueResult {
        public bool IsValid { get; set; }
        public string Value { get; set; }
        public string ResponseMessage { get; set; }
    }
}