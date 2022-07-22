using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace PodNoms.Common.Services.Startup {
    public static class HttpStartup {
        // private WebProxy localDebuggingProxy = new WebProxy("http://localhost:9537");

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
        public static IServiceCollection AddPodNomsHttpClients(
                    this IServiceCollection services,
                    IConfiguration config,
                    bool isProduction) {

            services.AddHttpClient("mixcloud", c => {
                c.BaseAddress = new Uri("https://api.mixcloud.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient("patreon", c => {
                c.BaseAddress = new Uri("https://www.patreon.com/api/");
            })/*.ConfigurePrimaryHttpMessageHandler(() => HttpProxyFactory.GetZapProxy())*/;

            services.AddHttpClient("youtube", c => {
                c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
                c.DefaultRequestHeaders.Add(
                    "Accept",
                    "*/*"
                );
            });

            services.AddHttpClient("unsplash", c => {
                c.BaseAddress = new Uri("https://api.unsplash.com/");
                c.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Client-ID {config.GetSection("ImageSettings")["UnsplashAccessKey"]}");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddHttpClient("StripeInvoices", c => {
                c.DefaultRequestHeaders.Add("Accept", "text/html");
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("RemotePageParser", c => {
                c.BaseAddress = new Uri(config.GetSection("AppSettings")["ScraperUrl"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.Timeout = TimeSpan.FromSeconds(20);
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("CachedAudio", c => {
                c.BaseAddress = new Uri(config.GetSection("AppSettings")["ApiUrl"]);
            }).ConfigurePodNomsHttpMessageHandler(isProduction);

            services.AddHttpClient("podnoms", c => {
                c.BaseAddress = new Uri(config["AppSettings:ApiUrl"]);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "PodNoms-JobProcessor");
            }).ConfigurePodNomsHttpMessageHandler(isProduction);

            services.AddHttpClient();
            return services;
        }
        private static IHttpClientBuilder ConfigurePodNomsHttpMessageHandler(this IHttpClientBuilder builder, bool isProduction) {
            if (!isProduction) {
                builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => {
                        return true;
                    }
                });
            }
            return builder;
        }
    }
}
