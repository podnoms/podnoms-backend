namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ProfileLimitsViewModel {
        public decimal StorageQuota { get; set; }
        public decimal StorageUsed { get; set; }
        public ProfileViewModel User { get; set; }
    }
}