using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Processor;

namespace PodNoms.Common.Services.Jobs;

public class UploadAudioJob : AbstractHostedJob {
  private readonly ILogger _logger;
  private readonly IAudioUploadProcessService _uploadProcessService;

  public UploadAudioJob(
    IAudioUploadProcessService uploadProcessService,
    ILogger<UploadAudioJob> logger, IMailSender mailSender) : base(logger) {
    _uploadProcessService = uploadProcessService;
    _logger = logger;
  }

  public override Task<bool> Execute(PerformContext context) {
    throw new NotImplementedException();
  }

  public async Task<bool> Execute(Guid entryId, string cacheFile, PerformContext context) {
    _setPerformContext(context);
    Log($"Starting Upload Job for {entryId} - {cacheFile}");

    if (File.Exists(cacheFile)) {
      return await _uploadProcessService.UploadAudio(entryId, cacheFile);
    }

    context.WriteLine($"Failed to cache remote file {cacheFile}");
    return false;
  }
}
