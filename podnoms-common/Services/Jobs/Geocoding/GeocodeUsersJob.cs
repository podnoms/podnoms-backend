using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Common.Data.Settings;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Jobs.Geocoding {

    public class GeocodeUsersJob : IHostedJob {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;

        public GeocodeUsersJob(UserManager<ApplicationUser> userManager, IHttpClientFactory httpClientFactory,
            ILogger<GeocodeUsersJob> logger, IOptions<AppSettings> appSettings) {
            _userManager = userManager;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ipstack_geocoder");
            _appSettings = appSettings.Value;
        }
        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute() { return await Execute(null); }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<bool> Execute(PerformContext context) {
            context.WriteLine("Starting to geocode users");
            context.WriteLine($"Key: {_appSettings.IPStackKey}");
            foreach (var user in _userManager.Users.Where(i => !string.IsNullOrEmpty(i.IpAddress))) {
                context.WriteLine($"Geoding: {user.Slug}");
                string url = $"http://api.ipstack.com/{user.IpAddress}?access_key={_appSettings.IPStackKey}";
                context.WriteLine($"Url: {url}");
                var response = await _httpClient.GetStringAsync(url);
                if (!string.IsNullOrEmpty(response)) {
                    context.WriteLine("Got a response");
                    try {
                        var result = JsonConvert.DeserializeObject<RootObject>(response);
                        if (result.country_code != null &&
                            result.country_name != null) {
                            context.WriteLine("Response is valid");
                            user.CountryCode = result.country_code;
                            user.CountryName = result.country_name;
                            user.RegionCode = result.region_code;
                            user.RegionName = result.region_name;
                            user.City = result.city;
                            user.Zip = result.zip;
                            user.Latitude = result.latitude;
                            user.Longitude = result.longitude;
                            await _userManager.UpdateAsync(user);
                        }
                    } catch (Exception ex) {
                        context.WriteLine($"Error coding {user.Id}");
                        context.WriteLine(ex.Message);
                    }
                }
            }
            return true;
        }
    }
}
