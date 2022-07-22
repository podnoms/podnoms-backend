using System;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class ChatMessage : BaseEntity, IEntity {

        public virtual ApplicationUser FromUser { get; set; }
        public virtual ApplicationUser ToUser { get; set; }
        public string Message { get; set; }
        public DateTime? MessageSeen { get; set; }

    }
}

