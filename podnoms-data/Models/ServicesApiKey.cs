using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PodNoms.Data.Models {
    public class ServicesApiKey : BaseEntity {
        [MaxLength(200)] public string Type { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        [MaxLength(100)] public string Key { get; set; }

        public virtual List<ServicesApiKeyLog> Usages { get; set; }
    }

    public class ServicesApiKeyLog : BaseEntity {
        public string Stack { get; set; }
        public ServicesApiKey ApiKey { get; set; }
        public ApplicationUser User { get; set; }
    }
}
