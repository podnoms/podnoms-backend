using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace PodNoms.Common.Services.Startup {
    public static class HttpStartup {
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
        public static IServiceCollection AddPodNomsHttpClients(this IServiceCollection services) {

            services.AddHttpClient("mixcloud", c => {
                c.BaseAddress = new Uri("https://api.mixcloud.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient("StripeInvoices", c => {
                c.DefaultRequestHeaders.Add("Accept", "text/html");

            }).AddPolicyHandler(GetRetryPolicy());
            services.AddHttpClient();

            return services;
        }


    }
}