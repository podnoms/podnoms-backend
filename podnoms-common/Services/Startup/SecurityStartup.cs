using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using PodNoms.Common.Auth;
using PodNoms.Identity.Data;

namespace PodNoms.Common.Services.Startup {
    public static class SecurityStartup {
        //TODO: Remove this from the binary

        public static IServiceCollection AddPodnomsSecurity(this IServiceCollection services, IConfiguration config) {
            var guestPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "podnoms-api-access")
                .Build();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<PodnomsAuthDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options => {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            // Register the OpenIddict validation components.
            services.AddOpenIddict()
                .AddValidation(options => {
                    // Note: the validation handler uses OpenID Connect discovery
                    // to retrieve the address of the introspection endpoint.
                    options.SetIssuer("https://dev-auth.pdnm.be:5003/");
                    options.AddAudiences("rs_podnoms-api-access");

                    // Configure the validation handler to use introspection and register the client
                    // credentials used when communicating with the remote introspection endpoint.
                    options.UseIntrospection()
                        .SetClientId("rs_podnoms-api-access")
                        .SetClientSecret("arglebarglefooferra");
                    options.UseSystemNetHttp();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            services.AddScoped<IAuthorizationHandler, RequireScopeHandler>();
            services.AddAuthorization(options => {
                options.AddPolicy("dataEventRecordsPolicy", policyUser => {
                    policyUser.Requirements.Add(new RequireScope());
                });
            });
            return services;
        }

        public static IServiceCollection AddPodNomsCors(this IServiceCollection services, IConfiguration config) {
            services.AddCors(options => {
                options.AddPolicy("DefaultCors", config => config
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

                options.AddPolicy("PodNomsClientPolicy", config => config
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(
                        //TODO: Will have to add all Podcast.CustomUrl values into here
                        "http://localhost:3000",
                        "http://localhost:4200",
                        "https://localhost:4200",
                        "https://localhost:5003",
                        "http://localhost:8080",
                        "http://localhost:8081",
                        "http://10.1.1.1:8080",
                        "https://dev.podnoms.com:4200",
                        "https://dev.pdnm.be:4200",
                        "http://dev.pdnm.be:8080",
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
            });
            return services;
        }

        public static IApplicationBuilder UsePodNomsCors(this IApplicationBuilder app) {
            app.UseCors();
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
