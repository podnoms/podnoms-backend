using Microsoft.AspNetCore.Identity;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Annotations;

namespace PodNoms.Api.Services.Auth {
    public class ApplicationUser : IdentityUser, ISluggedEntity {
        // Extended Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? FacebookId { get; set; }
        public string PictureUrl { get; set; }
        
        [SlugField(sourceField: "FullName")]
        public string Slug { get; set; }
        public string FullName { get => $"{FirstName} {LastName}"; }
    }
}