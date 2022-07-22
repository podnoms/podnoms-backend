using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.PageParser {
    public interface IPageParser {
        Task<bool> Initialise(string url);
        Task<Dictionary<string, string>> GetHeadTags();
        Task<string> GetPageTitle();
        Task<string> GetHeadTag(string tagName);
        Task<Dictionary<string, string>> GetAllAudioLinks(bool isDeep = false);
    }
}
