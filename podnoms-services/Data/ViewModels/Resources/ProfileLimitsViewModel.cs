using System;
using System.Numerics;

namespace PodNoms.Data.Models.ViewModels {
    public class ProfileLimitsViewModel {
        public decimal StorageQuota { get; set; }
        public decimal StorageUsed { get; set; }
        public ProfileViewModel User { get; set; }
    }
}