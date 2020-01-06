using System.Threading.Tasks;
using PodNoms.Common.Utils;
using TagLib;

namespace PodNoms.Common.Services.Audio {
    public interface IMP3Tagger {
        Task<bool> GenerateTags(string localFile, string title, string album, string imageUrl);

    }
    public class MP3Tagger : IMP3Tagger {
        public async Task<bool> GenerateTags(string localFile, string title, string album, string imageUrl) {
            TagLib.File file = TagLib.File.Create(localFile);
            file.RemoveTags(TagTypes.AllTags);
            file.Save();

            if (!string.IsNullOrEmpty(imageUrl)) {
                var localImageFile = await HttpUtils.DownloadFile(imageUrl);

                if (System.IO.File.Exists(localImageFile)) {
                    var image = new Picture(localImageFile);
                    file.Tag.Pictures = new[] { image };
                }
            }
            file.Tag.Title = title;
            file.Tag.Album = album;
            file.Save();
            return true;
        }
    }
}
