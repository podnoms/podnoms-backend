namespace PodNoms.Data.Models.ViewModels {
    public class NotificationOptionViewModel<T> {
        public T Value;
        public string Key;
        public string Label;
        public bool Required;
        public int Order;
        public string ControlType;

        public NotificationOptionViewModel(T value, string key, string label, bool required, int order, string controlType) {
            this.Value = value;
            this.Key = key;
            this.Label = label;
            this.Required = required;
            this.Order = order;
            this.ControlType = controlType;
        }
    }
}