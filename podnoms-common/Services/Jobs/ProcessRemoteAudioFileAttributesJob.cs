using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Storage;

namespace PodNoms.Common.Services.Jobs {
    public class ProcessRemoteAudioFileAttributesJob : IJob {
        private readonly IEntryRepository _entryRepository;
        private readonly IFileUtilities _fileUtilities;
        private readonly ILogger<ProcessRemoteAudioFileAttributesJob> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessRemoteAudioFileAttributesJob(IEntryRepository entryRepository,
                            IFileUtilities fileUtilities, ILogger<ProcessRemoteAudioFileAttributesJob> logger,
                            IUnitOfWork unitOfWork) {
            _logger = logger;
            _entryRepository = entryRepository;
            _fileUtilities = fileUtilities;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Execute() {
            var entries = _entryRepository.GetAll();

            foreach (var entry in entries) {
                var parts = entry.AudioUrl.Split("/");

                if (parts.Length == 2) {
                    _logger.LogInformation($"Processing remote: {entry.AudioUrl}");
                    try {
                        var size = await _fileUtilities.GetRemoteFileSize(
                            parts[0], parts[1]);
                        if (size != -1) {
                            entry.AudioFileSize = size;
                        }
                    } catch (InvalidOperationException ex) {
                        _logger.LogWarning(ex, "Probably missing item error processing remote file");
                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Fatal error processing remote file");
                    }
                }
            }
            await _unitOfWork.CompleteAsync();
            return false;
        }
    }
}