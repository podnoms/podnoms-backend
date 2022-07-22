using System;
using TagLib;

namespace PodNoms.Common.Services.Audio {
    public interface IMP3Tagger {
        string GetTags(string localFile);
        void ClearTags(string localFile);
        double GetDuration(string localFile);

        bool CreateTags(string localFile, string imageUrl, string title, string album, string author, string copyright,
            string comments);
    }

    public class MP3Tagger : IMP3Tagger {
        public string GetTags(string localFile) {
            using var file = TagLib.File.Create(localFile);
            return $"{file.Tag.Title}{Environment.NewLine}{file.Tag.Album}{Environment.NewLine}{file.Tag.Comment}";
        }

        public void ClearTags(string localFile) {
            using var file = TagLib.File.Create(localFile);
            file.RemoveTags(TagTypes.AllTags);
            file.Save();
        }

        public double GetDuration(string localFile) {
            using var file = TagLib.File.Create(localFile, TagLib.ReadStyle.Average);
            return file.Properties.Duration.TotalSeconds;
        }

        public bool CreateTags(string localFile, string localImageFile,
            string title, string album, string author, string copyright, string comments) {
            using var file = TagLib.File.Create(localFile);

            if (System.IO.File.Exists(localImageFile)) {
                var image = new Picture(localImageFile);
                file.Tag.Pictures = new IPicture[] {image};
            }

            file.Tag.Title = title;
            file.Tag.Album = album;
            file.Tag.Performers = new string[] {author};
            file.Tag.AlbumArtists = new string[] {author};
            file.Tag.Copyright = copyright;
            file.Tag.Comment = comments;
            file.Save();

            return true;
        }
    }
}
