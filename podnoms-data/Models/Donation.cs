using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class Donation : BaseEntity {
        public ApplicationUser AppUser { get; set; }
        public long Amount { get; set; }
    }
}
