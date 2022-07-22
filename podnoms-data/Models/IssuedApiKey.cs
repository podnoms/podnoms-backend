using System;
using System.ComponentModel.DataAnnotations;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class IssuedApiKey : BaseEntity, IEntity {
        public IssuedApiKey() {

        }
        public IssuedApiKey(ApplicationUser issuedTo, string name, string scopes,
                            string prefix, string key) {
            IssuedTo = issuedTo;
            Name = name;
            Scopes = scopes;
            Prefix = prefix;
            Key = key;
            IsValid = true;
        }
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        public string Name { get; set; }

        [MaxLength(7)] //so the user can recognise their key
        public string Prefix { get; set; }

        public string Key { get; set; }

        public string Scopes { get; set; }
        public bool IsValid { get; set; }
        public DateTime? Expires { get; set; }

        public virtual ApplicationUser IssuedTo { get; set; }

    }
}
