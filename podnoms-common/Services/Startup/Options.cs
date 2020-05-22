using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PodNoms.Common.Data.Settings;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Startup {
    public static class Options {
        public static IServiceCollection AddPodNomsOptions(this IServiceCollection services, IConfiguration config) {
            services.AddOptions();
            services.Configure<AppSettings>(config.GetSection("AppSettings"));
            services.Configure<StorageSettings>(config.GetSection("StorageSettings"));
            services.Configure<StripeSettings>(config.GetSection("StripeSettings"));
            services.Configure<ApiKeyAuthSettings>(config.GetSection("ApiKeyAuthSettings"));
            services.Configure<HelpersSettings>(config.GetSection("HelpersSettings"));
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<FacebookAuthSettings>(config.GetSection("FacebookAuthSettings"));
            services.Configure<ChatSettings>(config.GetSection("ChatSettings"));
            services.Configure<SharingSettings>(config.GetSection("SharingSettings"));
            services.Configure<TwitterStreamListenerSettings>(config.GetSection("TwitterStreamListenerSettings"));
            services.Configure<PaymentSettings>(config.GetSection("PaymentSettings"));
            services.Configure<ImageFileStorageSettings>(config.GetSection("ImageFileStorageSettings"));
            services.Configure<AudioFileStorageSettings>(config.GetSection("AudioFileStorageSettings"));
            services.Configure<WaveformDataFileStorageSettings>(config.GetSection("WaveformDataFileStorageSettings"));
            services.Configure<JwtIssuerOptions>(config.GetSection("JwtIssuerOptions"));
            services.Configure<FormOptions>(options => {
                // options.ValueCountLimit = 10;
                options.ValueLengthLimit = int.MaxValue;
                options.MemoryBufferThreshold = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
            });
            return services;
        }
    }
}
