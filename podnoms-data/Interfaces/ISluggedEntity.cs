namespace PodNoms.Data.Interfaces {
    public interface ISluggedEntity {
        string Slug { get; set; }
    }

    //TOOD: This should probably be in the services layer
}