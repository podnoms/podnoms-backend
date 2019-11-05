using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public abstract class AbstractHostedJob : IHostedJob {
        private PerformContext _context;
        private readonly ILogger _logger;

        protected AbstractHostedJob(ILogger logger) {
            _logger = logger;
        }

        public Task<bool> Execute() => Execute(null);
        public abstract Task<bool> Execute(PerformContext context);

        protected void _setContext(PerformContext context) {
            this._context = context;
        }

        protected void Log(string message) {
            _logger.LogInformation(message);
            if (_context != null) {
                _context.WriteLine(message);
            }
        }

        protected void LogWarning(string message) {
            _logger.LogWarning(message);
            if (_context != null) {
                _context.WriteLine(message);
                _context.SetTextColor(ConsoleTextColor.DarkYellow);
                _context.ResetTextColor();
            }
        }

        protected void LogError(string message) {
            _logger.LogError(message);
            if (_context != null) {
                _context.WriteLine(message);
                _context.SetTextColor(ConsoleTextColor.Red);
                _context.ResetTextColor();
            }
        }

        protected void LogError(string message, Exception ex) {
            LogError(message);
            LogError(ex.Message);
        }
    }
}
