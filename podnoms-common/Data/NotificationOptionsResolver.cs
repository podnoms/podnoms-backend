using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Data {
    internal class NotificationOptionsResolver : IMemberValueResolver<Notification, NotificationViewModel, string,
        List<NotificationOptionViewModel>> {
        public List<NotificationOptionViewModel> Resolve(Notification source, NotificationViewModel destination, 
                string sourceMember, List<NotificationOptionViewModel> destMember, ResolutionContext context) {
            var config = BaseNotificationConfig.GetConfig(source.Type);
            var stored = JsonConvert.DeserializeObject<IList<NotificationOptionViewModel>>(sourceMember)
                .Select(v => new NotificationOptionViewModel(
                    v.Value,
                    v.Key,
                    config.Options[v.Key].Label,
                    config.Options[v.Key].Description,
                    config.Options[v.Key].Required,
                    config.Options[v.Key].ControlType
                ));
            return stored.ToList();
        }
    }
}