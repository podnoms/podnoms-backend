using System.Threading.Tasks;

namespace PodNoms.AudioParsing.UrlParsers {
    public interface IUrlParser {
        Task<bool> IsMatch(string url);
    }
}
