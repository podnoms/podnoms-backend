using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PodNoms.Common.Services.Jobs;

namespace PodNoms.Jobs.Services {

    public class PodNomsApiNotificationService : INotifyJobCompleteService {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PodNomsApiNotificationService> _logger;
        private readonly IConfiguration _config;

        public PodNomsApiNotificationService(
                IHttpClientFactory httpClientFactory,
                ILogger<PodNomsApiNotificationService> logger,
                IConfiguration configuration) {
            _httpClient = httpClientFactory.CreateClient("podnoms");
            _logger = logger;
            _config = configuration;
        }

        public async Task NotifyUser(string token, string userId, string title, string body, string target, string image) {
            try {
                if (string.IsNullOrEmpty(token)) {
                    _logger.LogWarning("Unable to NotifiyUser as no valid auth token");
                    return;
                }
                var urlString = $"userId=userId&title={title}&body={body}&target={target}&image={image}";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await _httpClient.PostAsync(
                    $"notification/notifyuser?{urlString}",
                    null);

                _logger.LogDebug("Sending message", urlString);
                if (result.StatusCode == System.Net.HttpStatusCode.Accepted) {
                    _logger.LogInformation($"User notifications for {userId} sent succesfully");
                } else {
                    _logger.LogError($"Error sending notification, API call produced {result.StatusCode} ");
                    _logger.LogError(result.ReasonPhrase);
                }
            } catch (Exception ex) {
                _logger.LogError($"Unable to send notification to user\n{ex.Message}\n\t{ex.InnerException.Message}");
            }
        }

        public async Task SendCustomNotifications(string token, Guid podcastId, string title, string body, string url) {
            try {
                if (string.IsNullOrEmpty(token)) {
                    _logger.LogWarning("Unable to SendCustomNotifications as no valid auth token");
                    return;
                }
                var payload = JsonConvert.SerializeObject(new {
                    podcastId = podcastId.ToString(),
                    title = title,
                    url = url
                });
                _logger.LogDebug("Sending message", payload);
                var urlString = $"podcastId={podcastId}&title={title}&body={body}&url={url}";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await _httpClient.PostAsync(
                    $"notification/sendcustomnotifications?{urlString}",
                    null);
                if (result.StatusCode == System.Net.HttpStatusCode.Accepted) {
                    _logger.LogInformation($"Custom notifications for {podcastId} sent succesfully");
                } else {
                    _logger.LogError($"Error sending notification, API call produced {result.StatusCode} ");
                    _logger.LogError(result.ReasonPhrase);
                }
            } catch (Exception ex) {
                _logger.LogError($"Unable to send notification to user\n{ex.Message}");
            }
        }
    }

}
