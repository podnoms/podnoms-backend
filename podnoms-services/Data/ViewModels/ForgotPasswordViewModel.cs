using System.ComponentModel.DataAnnotations;

namespace PodNoms.Data.Models.ViewModels {
    public class ForgotPasswordViewModel {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}