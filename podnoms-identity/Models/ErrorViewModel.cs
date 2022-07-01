using System.ComponentModel.DataAnnotations;

namespace PodNoms.Identity.Models;

public class ErrorViewModel {
    [Display(Name = "Error")] public string Error { get; set; }

    [Display(Name = "Description")] public string ErrorDescription { get; set; }
}
