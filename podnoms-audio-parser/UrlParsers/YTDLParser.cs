using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;
using PodNoms.AudioParsing.Helpers;

namespace PodNoms.AudioParsing.UrlParsers {
    public class YTDLParser : IUrlParser {
        public async Task<bool> IsMatch(string url) {
            try {
                var stdOutBuffer = new StringBuilder();
                var stdErrBuffer = new StringBuilder();
                //TODO Don't call YTDL here, refactor to class/module
                var result = await Cli.Wrap("youtube-dl")
                    .WithArguments($"{url} -j")
                    .WithValidation(CommandResultValidation.None)
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                    .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                    .ExecuteAsync();

                //according to this, youtube-dl HResult = {int} -2146233088 returns 0 with the above command if we can parse
                // https://github.com/ytdl-org/youtube-dl/issues/4503#issuecomment-67775061
                return stdOutBuffer.ToString().IsJson();
            } catch (CommandExecutionException e) {
            }

            return false;
        }

        public async Task<UrlType> GetType(string url) => await IsMatch(url) ? UrlType.YtDl : UrlType.Invalid;
    }
}
