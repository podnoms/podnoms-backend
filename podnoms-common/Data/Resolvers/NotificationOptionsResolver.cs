using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Data.Resolvers {
    internal class
        NotificationOptionsResolver : IValueResolver<Notification, NotificationViewModel,
            List<NotificationOptionViewModel>> {
        public List<NotificationOptionViewModel> Resolve(Notification source, NotificationViewModel destination,
            List<NotificationOptionViewModel> destMember, ResolutionContext context) {
            var config = BaseNotificationConfig.GetConfig(source.Type);
            var stored = JsonSerializer.Deserialize<IList<NotificationOptionViewModel>>(source.Config)
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
