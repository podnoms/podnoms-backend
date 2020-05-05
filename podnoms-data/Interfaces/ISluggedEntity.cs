namespace PodNoms.Data.Interfaces {
    public interface IUniqueFieldEntity {
    }
    public interface ISluggedEntity : IUniqueFieldEntity {
        string Slug { get; set; }
    }
}
