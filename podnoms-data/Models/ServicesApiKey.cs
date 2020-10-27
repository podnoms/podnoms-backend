using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PodNoms.Data.Models {
    public class ServicesApiKey : BaseEntity {
        [MaxLength(200)] public string Type { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        [MaxLength(100)] public string Key { get; set; }
        [MaxLength(2048)] public string Url { get; set; }

        public virtual List<ServicesApiKeyLog> Usages { get; set; } = new List<ServicesApiKeyLog>();

        public ServicesApiKeyLog LogRequest(string requesterId, string stackTrace) {
            var log = new ServicesApiKeyLog(this, requesterId, stackTrace);
            Usages.Add(log);
            return log;
        }
    }
}
