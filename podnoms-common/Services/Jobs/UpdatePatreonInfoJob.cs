using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Jobs {

    public class UpdatePatreonInfoJob : AbstractHostedJob {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdatePatreonInfoJob(ILogger<UpdatePatreonInfoJob> logger,
                IHttpClientFactory httpClientFactory) : base(logger) {
            _httpClientFactory = httpClientFactory;
        }

        public override Task<bool> Execute(PerformContext context) {
            throw new NotImplementedException();
        }
    }
}
