using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class Donation : IEntity {
        public Guid Id { get; set; }
        public ApplicationUser AppUser { get; set; }
        public long Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
