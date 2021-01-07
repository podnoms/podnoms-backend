using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;

namespace PodNoms.AudioParsing.UrlParsers {
    public class YTDLParser : IUrlParser {
        public async Task<bool> IsMatch(string url) {
            try {
                //TODO Don't call YTDL here, refactor to class/module
                var result = await Cli.Wrap("youtube-dl")
                    .WithArguments($"{url} -j")
                    .ExecuteAsync();
                //according to this, youtube-dl returns 0 with the above command if we can parse
                // https://github.com/ytdl-org/youtube-dl/issues/4503#issuecomment-67775061
                return result.ExitCode.Equals(0);
            } catch (CommandExecutionException) {
                return false;
            }
        }
    }
}
