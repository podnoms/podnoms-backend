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
        private readonly string _hubUrl;
        private readonly ILogger _logger;

        public SignalRClientUpdater(IConfiguration config, ILogger<SignalRClientUpdater> logger) {
            _hubUrl = config["JobHubs:AudioProcessingHub"];
            _logger = logger;
        }
        private async Task<bool> _initialiseConnection(string token) {
            _logger.LogDebug($"Starting hub: {_hubUrl}");
            var handler = new HttpClientHandler {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
            var url = $"{_hubUrl}?token={token.Replace("Bearer ", string.Empty)}";
            _connection = new HubConnectionBuilder()
                .WithUrl(url, options => {
                    //TODO: this should be dev only
                    options.HttpMessageHandlerFactory = h => handler;
                })
                .Build();

            //reconnect if closed
            _connection.Closed += async (error) => {
                _logger.LogDebug("Connection closed");

                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
                _logger.LogDebug("Reopened connection");
            };
            await _connection.StartAsync();
            _logger.LogDebug("Opened SignalR hub connection");

            return _connection.State == HubConnectionState.Connected;
        }

        public async Task<bool> SendProcessUpdate(string userId, string channel, object data) {
            //this will be used by the jobs worker
            //sends to the ASP.Net hub, which will then send to the 
            //appropriate user
            //userId is authToken
            if (string.IsNullOrEmpty(userId)) {
                _logger.LogDebug("User id was not supplied to SendProcessUpdate");
                return false;
            }
            try {
                if (_connection == null || _connection.State != HubConnectionState.Connected) {
                    if (!await _initialiseConnection(userId)) {
                        return false;
                    }
                }
                if (_connection.State == HubConnectionState.Connected) {
                    await _connection.InvokeAsync(
                        "Send", channel, data
                    );
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
