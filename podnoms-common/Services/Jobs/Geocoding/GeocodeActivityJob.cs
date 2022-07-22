using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs.Geocoding {
    public class GeocodeActivityJob : IHostedJob {
        private readonly HttpClient _httpClient;
        private readonly IRepoAccessor _repo;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;

        public GeocodeActivityJob(IActivityLogPodcastEntryRepository repository,
            IRepoAccessor repo,
            IHttpClientFactory httpClientFactory,
            ILogger<GeocodeActivityJob> logger, IOptions<AppSettings> appSettings) {
            _repo = repo;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ipstack_geocoder");
            _appSettings = appSettings.Value;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            return await Task.Run(() => {
                context.WriteLine("Geocoding disabled due to ipstack down");
                return false;
            });
            // context.WriteLine("Starting to geocode users");
            // context.WriteLine("Starting to geocode users");
            // context.WriteLine($"Key: {_appSettings.IPStackKey}");
            // var records = _repository
            //     .GetAll()
            //     .Where(i => !string.IsNullOrEmpty(i.ClientAddress) && string.IsNullOrEmpty(i.CountryCode));
            // foreach (var record in records) {
            //     await GeocodeActivityItem(record, context);
            // }
            // return true;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> GeocodeActivityItem(Guid activityId, PerformContext context) {
            var record = await _repo.ActivityLogPodcastEntry.GetAsync(activityId);
            return await GeocodeActivityItem(record, context);
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> GeocodeActivityItem(ActivityLogPodcastEntry record, PerformContext context) {
            context.WriteLine($"Geoding: {record.Id}");
            string url = $"http://api.ipstack.com/{record.ClientAddress}?access_key={_appSettings.IPStackKey}";
            context.WriteLine($"Url: {url}");
            var response = await _httpClient.GetStringAsync(url);
            if (!string.IsNullOrEmpty(response)) {
                context.WriteLine("Got a response");
                try {
                    var result = JsonSerializer.Deserialize<RootObject>(response);
                    if (result.country_code != null &&
                        result.country_name != null) {
                        context.WriteLine("Response is valid");
                        record.CountryCode = result.country_code;
                        record.CountryName = result.country_name;
                        record.RegionCode = result.region_code;
                        record.RegionName = result.region_name;
                        record.City = result.city;
                        record.Zip = result.zip;
                        record.Latitude = result.latitude;
                        record.Longitude = result.longitude;
                        await _repo.CompleteAsync();
                        return true;
                    }
                } catch (Exception ex) {
                    context.WriteLine($"Error coding {record.Id}");
                    context.WriteLine(ex.Message);
                }
            }

            return false;
        }
    }
}
