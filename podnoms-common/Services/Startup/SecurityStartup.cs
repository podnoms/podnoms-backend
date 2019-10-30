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
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Startup {
    public static class SecurityStartup {
        private const string SecretKey = "QGfaEMNASkNMGLKA3LjgPdkPfFEy3n40";

        private static readonly SymmetricSecurityKey
            SigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        public static IServiceCollection AddPodnomsSecurity(this IServiceCollection services, IConfiguration config) {
            var jwtAppSettingOptions = config.GetSection(nameof(JwtIssuerOptions));
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options => {
                //TODO: Remove this in production, only for testing
                options.ValidFor = TimeSpan.FromDays(28);
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
            });
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions => {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
                configureOptions.Events = new JwtBearerEvents {
                    OnMessageReceived = context => {
                        var accessToken = context.Request.Query["token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs"))) {
                            Console.WriteLine($"WE GOT THE TOKEN: {accessToken}");
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
                            "https://localhost:4200",
                            "http://localhost:8080",
                            "http://localhost:4200",
                            "http://10.1.1.5:9999",
                            "https://dev.podnoms.com:4200",
                            "https://podnoms.local:4200",
                            "https://podnoms.com",
                            "https://pages.podnoms.com",
                            "https://www.podnoms.com")
                        .AllowCredentials());
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
                            .CustomSources("https://cdn.podnoms.com/"))
                        .FontSources(s => s
                            .Self()
                            .CustomSources("https://cdn.podnoms.com/"))
                        .FormActions(s => s.Self())
                        .FrameAncestors(s => s.Self().
                            CustomSources("https://dl.pdnm.be/"))
                        .ImageSources(s => s.Self()
                            .CustomSources(
                                "https://cdn.podnoms.com/",
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
