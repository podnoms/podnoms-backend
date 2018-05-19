using System;

namespace PodNoms.Api.Models {
    public interface IEntity {
        Guid Id { get; set; }
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}