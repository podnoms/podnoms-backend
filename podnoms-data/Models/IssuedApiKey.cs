using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Utils;

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
