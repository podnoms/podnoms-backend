using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Realtime {
    public class SignalRClientUpdater : IRealTimeUpdater {
        private HubConnection _connection;

        private readonly string _token;
        private readonly string _hubUrl;
        private readonly ILogger _logger;

        public SignalRClientUpdater(IConfiguration config, ILogger<SignalRClientUpdater> logger) {
            _token = config["JobHubs:HubKey"];
            _hubUrl = config["JobHubs:AudioProcessingHub"];
            _logger = logger;
            _buildHub();
        }
        private void _buildHub() {
            _logger.LogDebug($"Starting hub: {_hubUrl}");
            var url = $"{_hubUrl}?rttkn={_token}";
            _logger.LogInformation($"Building hub for {url}");
            var handler = new HttpClientHandler {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
            _connection = new HubConnectionBuilder()
                    .WithUrl(url, options => {
                        //TODO: this should be dev only
                        options.HttpMessageHandlerFactory = h => handler;
                    })
                    .Build();
        }
        private async Task<bool> _initialiseConnection() {
            try {
                if (_connection == null || _connection.State != HubConnectionState.Connected) {
                    if (!await _initialiseConnection()) {
                        return false;
                    }
                }
            } catch (Exception e) {
                _logger.LogError($"Error starting signalR updater hub.\r\t{e.Message}");
            }
            _logger.LogDebug("Opened SignalR hub connection");

            return _connection.State == HubConnectionState.Connected;
        }

        public async Task<bool> SendProcessUpdate(string userId, string channel, object data) {
            if (string.IsNullOrEmpty(userId)) {
                _logger.LogDebug("User id was not supplied to SendProcessUpdate");
                return false;
            }
            try {
                if (!await _initialiseConnection()) {
                    _logger.LogError($"Unable to open SignalR hub: \n\tUrl:{_hubUrl}\n\tToken: {_token}");
                    return false;
                }
                if (_connection.State == HubConnectionState.Connected) {
                    _logger.LogInformation("Connection is open");
                    await _connection.InvokeAsync(
                        "Send", channel, data
                    );
                    _logger.LogInformation($"Message successfully sent\n{data}");

                    return true;
                }
            } catch (Exception ex) {
                _logger.LogError($"Error sending process update\n{ex?.Message}\n{ex?.InnerException.Message}");
                //return true here as we don't want any signalr errors interfering with audio processing.
                return true;
            }
            return false;
        }
    }
}
