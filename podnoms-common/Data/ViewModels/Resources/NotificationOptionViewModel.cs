namespace PodNoms.Common.Data.ViewModels.Resources {
    public class NotificationOptionViewModel {
        public string Value { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string ControlType { get; set; }

        public NotificationOptionViewModel(string value, string key, string label, string description, bool required,
            string controlType) {
            Value = value;
            Key = key;
            Label = label;
            Description = description;
            Required = required;
            ControlType = controlType;
        }
    }
}