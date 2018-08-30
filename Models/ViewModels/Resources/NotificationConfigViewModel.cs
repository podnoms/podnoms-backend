using System;
using System.Collections.Generic;

namespace PodNoms.Api.Models.ViewModels {
    public class NotificationConfigViewModel {
        public string Type { get; set; }
        public string ValueType { get; set; }
        public List<NotificationConfigViewItem<string>> Configs;
    }

    public class NotificationConfigViewItem<T> {
        public T Value;
        public string Key;
        public string Label;
        public bool Required;
        public int Order;
        public string ControlType;

        public NotificationConfigViewItem(T value, string key, string label, bool required, int order, string controlType) {
            this.Value = value;
            this.Key = key;
            this.Label = label;
            this.Required = required;
            this.Order = order;
            this.ControlType = controlType;
        }
    }
}