namespace PodNoms.Common.Data.ViewModels.Resources {
    public class NotificationOptionViewModel<T> {
        public T Value;
        public string Key;
        public string Label;
        public bool Required;
        public int Order;
        public string ControlType;

        public NotificationOptionViewModel() {
        }
        public NotificationOptionViewModel(T value, string key, string label, bool required, int order, string controlType) {
            Value = value;
            Key = key;
            Label = label;
            Required = required;
            Order = order;
            ControlType = controlType;
        }
    }
}