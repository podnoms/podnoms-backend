using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {

    // public class UpdatePatreonInfoJob : AbstractHostedJob {
    //     private readonly ILogger<UpdatePatreonInfoJob> _logger;

    //     public UpdatePatreonInfoJob(ILogger<UpdatePatreonInfoJob> logger) {
    //         _logger = logger;
    //     }

    //     public UpdatePatreonInfoJob(ILogger logger) : base(logger) {
    //     }

    //     public override Task<bool> Execute(PerformContext context) {
    //         throw new NotImplementedException();
    //     }
    // }
}
