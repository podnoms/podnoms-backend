using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    public class BoilerPlate : BaseEntity, IEntity {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}

