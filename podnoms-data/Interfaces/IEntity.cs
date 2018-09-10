using System;

namespace PodNoms.Data.Models {
    public interface IEntity {
        Guid Id { get; set; }
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}