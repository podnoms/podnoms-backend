using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PodNoms.Data.Models {
    public class ServiceApiKeyViewModel {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
    }


    public class ServiceApiKey : BaseEntity {
        public static int TAINT_LENGTH = 2;
        [MaxLength(200)] public string Type { get; set; }
        [MaxLength(1000)] public string Description { get; set; }
        [MaxLength(100)] public string Key { get; set; }
        [MaxLength(2048)] public string Url { get; set; }
        public bool Enabled { get; set; } = true;

        public Guid? ApplicationUserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// If a key is logged as invalid we can store the date here
        /// and also the reason, we can then not use this key for a period
        /// TODO: We should probably create a job that re-checks all keys
        /// that have been marked invalid
        /// </summary>
        public bool Tainted { get; set; } = false;

        public DateTime? TaintedDate { get; set; }
        public string TaintedReason { get; set; }

        public virtual List<ServicesApiKeyLog> Usages { get; set; } = new List<ServicesApiKeyLog>();

        public ServicesApiKeyLog LogRequest(string requesterId, string stackTrace) {
            var log = new ServicesApiKeyLog(this, requesterId, stackTrace);
            Usages.Add(log);
            return log;
        }

        public bool Equals(ServiceApiKey other) {
            return other != null &&
                   Type == other.Type &&
                   Description == other.Description &&
                   Key == other.Key &&
                   Url == other.Url &&
                   Equals(Usages, other.Usages);
        }
    }
}
