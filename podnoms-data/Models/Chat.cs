using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class ChatMessage : BaseEntity, IEntity {

        public virtual ApplicationUser FromUser { get; set; }
        public virtual ApplicationUser ToUser { get; set; }
        public string Message { get; set; }
        public DateTime? MessageSeen { get; set; }

    }
}

