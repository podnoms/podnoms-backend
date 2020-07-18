using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using PodNoms.Common.Services.Payments.Patreon.Models;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Payments.Patreon {
    public class PatreonClient : IDisposable {
        public const string SAFE_ROOT = "https://www.patreon.com/oauth2/authorize";
        public const string PUBLIC_ROOT = "https://www.patreon.com";
        private readonly HttpClient _httpClient;
        private readonly PatreonSettings _config;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILogger<PatreonClient> _logger;

        public static string CampaignURL(string campaignId) => SAFE_ROOT + $"campaigns/{campaignId}/";
        public static string PledgesURL(string campaignId) => CampaignURL(campaignId) + "pledges";

        public static string UserURL(string userId) => PUBLIC_ROOT + "user/" + userId;

        public static string PLEDGE_FIELDS => "fields%5Bpledge%5D=amount_cents,created_at,declined_since,pledge_cap_cents,patron_pays_fees,total_historical_amount_cents,is_paused,has_shipping_address";


        public PatreonClient(IOptions<PatreonSettings> config, IOptions<PaymentSettings> paymentSettings, IHttpClientFactory clientFactory, ILogger<PatreonClient> logger) {
            _httpClient = clientFactory.CreateClient();
            _config = config.Value;
            _paymentSettings = paymentSettings.Value;
            _logger = logger;
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }
}
