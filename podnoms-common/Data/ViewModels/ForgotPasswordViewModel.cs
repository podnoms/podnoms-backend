using System.ComponentModel.DataAnnotations;

namespace PodNoms.Common.Data.ViewModels {
    public class ForgotPasswordViewModel {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}