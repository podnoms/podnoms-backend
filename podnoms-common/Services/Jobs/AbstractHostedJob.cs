using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public abstract class AbstractHostedJob : IHostedJob {
        private PerformContext _context;
        private readonly ILogger<AbstractHostedJob> _logger;

        protected AbstractHostedJob(ILogger<AbstractHostedJob> logger) {
            _logger = logger;
        }

        public Task<bool> Execute() => Execute(null);
        public abstract Task<bool> Execute(PerformContext context);

        protected void _setPerformContext(PerformContext context) {
            this._context = context;
        }

        private void _logToContext(string message, ConsoleTextColor color) {
            _context.WriteLine(message);
            _context.SetTextColor(color);
            _context.ResetTextColor();
        }

        protected void Log(string message) {
            _logger.LogInformation("{Message}", message);
            _logToContext(message, ConsoleTextColor.White);
        }

        protected void LogWarning(string message) {
            _logger.LogWarning("{Message}", message);
            _logToContext(message, ConsoleTextColor.Yellow);
        }

        protected void LogError(string message) {
            _logger.LogError("{Message}", message);
            _logToContext(message, ConsoleTextColor.Red);
        }

        protected void LogError(string message, Exception ex) {
            LogError(message);
            LogError(ex.Message);
        }

        protected void LogDebug(string message) {
            _logger.LogDebug("{Message}", message);
            _logToContext(message, ConsoleTextColor.Green);
        }
    }
}
