using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PodNoms.Common.Auth;
using PodNoms.Common.Auth.ApiKeys;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Startup {
    public static class SecurityStartup {
        //TODO: Remove this from the binary

        public static IServiceCollection AddPodnomsSecurity(this IServiceCollection services, IConfiguration config) {
            var jwtAppSettingOptions = config.GetSection(nameof(JwtIssuerOptions));
            SymmetricSecurityKey signingKey =
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions["SigningKey"]));
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options => {
                //TODO: Remove this in production, only for testing
                // options.ValidFor = TimeSpan.FromDays(28);
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddApiKeySupport(options => { })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions => {
                    configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                    configureOptions.TokenValidationParameters = tokenValidationParameters;
                    configureOptions.SaveToken = true;
                    configureOptions.Events = new JwtBearerEvents {
                        OnMessageReceived = context => {
                            var accessToken = context.Request.Query["token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs"))) {
                                context.Token = accessToken[0];
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(j => {
                j.AddPolicy("ApiUser", policy => policy.RequireClaim(
                    Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });
            // add identity
            services.AddIdentityCore<ApplicationUser>(o => {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<PodNomsDbContext>().AddDefaultTokenProviders()
                .AddUserManager<PodNomsUserManager>();

            return services;
        }

        public static IServiceCollection AddPodNomsCors(this IServiceCollection services, IConfiguration config) {
            services.AddCors(options => {
                options.AddPolicy("PodNomsClientPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(
                            //TODO: Will have to add all Podcast.CustomUrl values into here
                            "http://localhost:8080",
                            "http://localhost:8080",
                            "http://localhost:4200",
                            "https://localhost:4200",
                            "https://localhost:5003",
                            "https://dev.podnoms.com:4200",
                            "https://podnoms.local:4200",
                            "http://10.1.1.1:8080",
                            "http://podnoms.local:8080",
                            "https://podnoms.com",
                            "chrome-extension://ckjjhlmhcdeneallemnklpdbkneinepf",
                            "chrome-extension://idhfpcbfcbppfngmhidbaimgefdjoljh",
                            "chrome-extension://eildkhlkeklepmmjddhlnokmmfgiafad",
                            "moz-extension://2a6bcbb2-6ee5-46ef-8886-50a1af61be5d",
                            "moz-extension://1f5f96b0-52cb-4541-bbe1-cd7bad43cd6b",
                            "moz-extension://ed9b8e44-a00e-4be1-b082-b00069a474e5",
                            "moz-extension://19c29fcf-033c-43aa-8b36-b49a702a1708",
                            "moz-extension://002c342a-efa6-4c69-949b-b61650926f42",
                            "https://www.podnoms.com")
                        .AllowCredentials());
                options.AddPolicy("PublicApiPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
                options.AddPolicy("BrowserExtensionPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });
            return services;
        }

        public static IApplicationBuilder UsePodNomsCors(this IApplicationBuilder app) {
            app.UseCors("PodNomsClientPolicy");
            return app;
        }

        public static IApplicationBuilder UseSecureHeaders(
            this IApplicationBuilder app, bool isDevelopment) {
            if (!isDevelopment) {
                app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());

                app.UseXContentTypeOptions();
                app.UseReferrerPolicy(opts => opts.NoReferrer());
                app.UseXXssProtection(options => options.EnabledWithBlockMode());
                app.UseXfo(options => options.Deny());
                app.UseCsp(opts => opts
                    .BlockAllMixedContent()
                    .StyleSources(s => s
                        .Self()
                        .UnsafeInline()
                        .CustomSources(
                            "https://cdn.podnoms.com/",
                            "https://fonts.googleapis.com/",
                            "https://cdnjs.cloudflare.com/",
                            "https://stackpath.bootstrapcdn.com/"))
                    .FontSources(s => s
                        .Self()
                        .CustomSources(
                            "https://cdn.podnoms.com/",
                            "https://fonts.googleapis.com/",
                            "https://fonts.gstatic.com/",
                            "https://cdnjs.cloudflare.com"))
                    .FormActions(s => s.Self())
                    .FrameAncestors(s => s.Self().CustomSources("https://dl.pdnm.be/"))
                    .ImageSources(s => s.Self()
                        .CustomSources(
                            "https://cdn.podnoms.com/",
                            "https://i.pdnm.be/",
                            "https://cdn-l.podnoms.com/"))
                    .ScriptSources(s => s
                        .Self()
                        .CustomSources("https://cdn.podnoms.com/player/")
                        .UnsafeInline())
                );
            }

            return app;
        }
    }
}
