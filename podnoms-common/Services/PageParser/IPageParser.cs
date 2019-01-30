using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.PageParser {
    public interface IPageParser {
        Task<IList<KeyValuePair<string, string>>> GetAllAudioLinks(string url);
        Task<IList<ILookup<string, string>>> GetIFrameLinks(string url);
        Task<ILookup<string, string>> GetAudioLinks(string url);
    }
}
