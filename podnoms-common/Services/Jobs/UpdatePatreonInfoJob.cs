using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Hangfire.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels.Remote.Patreon;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs {
    public class UpdatePatreonInfoJob : AbstractHostedJob {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepoAccessor _repo;
        private readonly PatreonSettings _patreonSettings;

        public UpdatePatreonInfoJob(ILogger<UpdatePatreonInfoJob> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<PatreonSettings> patreonSettings,
            UserManager<ApplicationUser> userManager,
            IRepoAccessor repo) : base(logger) {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _repo = repo;
            _patreonSettings = patreonSettings.Value;
        }

        public override async Task<bool> Execute(PerformContext context) {
            var subscriptionRepository = _repo.CreateProxy<AccountSubscription>();
            var tokens = await _repo.CreateProxy<PatreonToken>()
                .GetAll()
                .ToListAsync();
            foreach (var token in tokens) {
                try {
                    var user = await _userManager.FindByIdAsync(token.AppUserId);
                    if (user is null) {
                        LogError($"Unable to find user for token: {token.Id}");
                        continue;
                    }

                    using var client = _httpClientFactory.CreateClient("patreon");

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    var response = await client.GetAsync(
                        "/api/oauth2/v2/identity?include=memberships,memberships.currently_entitled_tiers"
                    );
                    response.EnsureSuccessStatusCode();

                    var data = await response.Content.ReadAsStringAsync();
                    var ds = JsonSerializer.Deserialize<PatreonResponse>(data);

                    if (ds.Subscriptions is null)
                        continue;

                    // check if the user has subscribed to any of our active campaigns
                    foreach (var userTiers in ds.Subscriptions
                                 .Where(t => t.Id.Equals(_patreonSettings.CampaignId))
                                 .Select(t => t.RelationShips?.CurrentlyEntitledTiers)) {
                        foreach (var t in userTiers?.Tiers) {
                            Log($"Found active tier: {t.Id} - {t.Type}");
                            var subType = _patreonSettings.Tiers?
                                .FirstOrDefault(patreonTier => patreonTier.Id.Equals((t.Id)));
                            if (subType == null) continue;

                            var sub = new AccountSubscription {
                                Type = AccountSubscriptionType.Patreon,
                                Tier = subType.Level,
                                EndDate = DateTime.Today.AddDays(1),
                                WasSuccessful = true
                            };
                            var existingSubs = subscriptionRepository
                                .GetAll()
                                .Where(r => r.Type.Equals(AccountSubscriptionType.Patreon));
                            _repo.Context.RemoveRange(
                                existingSubs
                            );
                            user.AccountSubscriptions.Add(sub);
                            await _repo.CompleteAsync();
                        }
                    }
                } catch (Exception e) {
                    LogError($"Error updating patreon info: {e.Message}");
                }
            }

            return true;
        }
    }
}
