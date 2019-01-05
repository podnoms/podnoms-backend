using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.PageParser {
    public interface IPageParser {
        Task<Dictionary<string, string>> GetAudioLinks(string url);
    }
}