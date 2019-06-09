using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.AspNetCore;
using Google.Apis.Util;
using Hangfire;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PodNoms.Api.Providers;
using PodNoms.Common.Auth;
using PodNoms.Common.Data;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Jobs;
using PodNoms.Common.Services.Logging;
using PodNoms.Common.Services.Middleware;
using PodNoms.Common.Services.Push.Extensions;
using PodNoms.Common.Services.Startup;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace PodNoms.Api {
    public class Startup {
        private const string SecretKey = "QGfaEMNASkNMGLKA3LjgPdkPfFEy3n40";

        private readonly SymmetricSecurityKey
        _signingKey = new SymmetricSecurityKey (Encoding.ASCII.GetBytes (SecretKey));

        private static Mutex mutex = new Mutex ();
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Env { get; }

        public Startup (IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices (IServiceCollection services) {
            Console.WriteLine ($"Configuring services: {Configuration}");

            services.AddPodNomsOptions (Configuration);
            services.AddPodNomsHealthChecks (Configuration);
            services.AddPodNomsApplicationInsights (Configuration, Env.IsProduction ());

            mutex.WaitOne ();
            Mapper.Reset ();
            services.AddAutoMapper (
                e => { e.AddProfile (new MappingProvider (Configuration)); },
                AppDomain.CurrentDomain.GetAssemblies ());
            mutex.ReleaseMutex ();

            services.AddDbContext<PodNomsDbContext> (options => {
                options.UseSqlServer (
                    Configuration.GetConnectionString ("DefaultConnection"),
                    b => b.MigrationsAssembly ("podnoms-common"));
            });

            services.AddHealthChecks ();
            services.AddPodNomsHttpClients ();

            var jwtAppSettingOptions = Configuration.GetSection (nameof (JwtIssuerOptions));
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions> (options => {
                //TODO: Remove this in production, only for testing
                options.ValidFor = TimeSpan.FromDays (28);
                options.Issuer = jwtAppSettingOptions[nameof (JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof (JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials (_signingKey, SecurityAlgorithms.HmacSha256);
            });
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof (JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof (JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication (options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer (configureOptions => {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof (JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
                configureOptions.Events = new JwtBearerEvents {
                    OnMessageReceived = context => {
                        if (context.Request.Path.Value.StartsWith ("/hubs/") &&
                            context.Request.Query.TryGetValue ("token", out var token)) {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization (j => {
                j.AddPolicy ("ApiUser", policy => policy.RequireClaim (
                    Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });
            // add identity
            var identityBuilder = services.AddIdentityCore<ApplicationUser> (o => {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
            });
            identityBuilder =
                new IdentityBuilder (identityBuilder.UserType, typeof (IdentityRole), identityBuilder.Services);
            identityBuilder.AddEntityFrameworkStores<PodNomsDbContext> ().AddDefaultTokenProviders ();
            identityBuilder.AddUserManager<PodNomsUserManager> ();

            services.AddMvc (options => {
                    //TODO: This needs to be investigated
                    options.EnableEndpointRouting = false;
                    options.OutputFormatters.Add (new XmlSerializerOutputFormatter ());
                    options.OutputFormatters
                        .OfType<StringOutputFormatter> ()
                        .Single ().SupportedMediaTypes.Add ("text/html");
                })
                .SetCompatibilityVersion (CompatibilityVersion.Latest)
                .AddJsonOptions (options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver ();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                    options.SerializerSettings.Converters.Add (new Newtonsoft.Json.Converters.StringEnumConverter ());
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddXmlSerializerFormatters ()
                .AddFluentValidation (fv => fv.RegisterValidatorsFromAssemblyContaining<Startup> ());

            services.AddSwaggerGen (c => {
                c.SwaggerDoc ("v1", new Info { Title = "PodNoms.API", Version = "v1" });
                c.DocumentFilter<LowercaseDocumentFilter> ();
            });

            services.Configure<FormOptions> (x => {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            services.AddCors (options => {
                options.AddPolicy ("PodNomsClientPolicy",
                    builder => builder
                    .AllowAnyMethod ()
                    .AllowAnyHeader ()
                    .WithOrigins (
                        "https://localhost:4200",
                        "http://localhost:8080",
                        "http://localhost:4200",
                        "http://10.1.1.5:9999",
                        "https://dev.podnoms.com:4200",
                        "https://podnoms.com",
                        "https://pages.podnoms.com",
                        "https://www.podnoms.com")
                    .AllowCredentials ());
                options.AddPolicy ("BrowserExtensionPolicy",
                    builder => builder
                    .AllowAnyOrigin ()
                    .AllowAnyHeader ()
                    .AllowAnyMethod ());
            });
            services.AddPodNomsImaging (Configuration);
            services.AddPushSubscriptionStore (Configuration);
            services.AddPushNotificationService (Configuration);

            services.AddPodNomsSignalR ();
            services.AddDependencies ();
            services.AddPodNomsHangfire (Configuration, Env.IsProduction ());

            //register the codepages (required for slugify)
            var instance = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider (instance);
        }

        public void Configure (IApplicationBuilder app, ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider, IApplicationLifetime lifetime) {

            UpdateDatabase (app);

            app.UseHttpStatusCodeExceptionMiddleware ();
            app.UseHttpsRedirection ();

            app.UseExceptionHandler (new ExceptionHandlerOptions {
                ExceptionHandler = new JsonExceptionMiddleware (Env).Invoke
            });

            app.UseSqlitePushSubscriptionStore ();

            app.UseCustomDomainRewrites ();
            app.UseStaticFiles ();

            app.UseForwardedHeaders (new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication ();

            app.UseCors ("PodNomsClientPolicy");

            app.UseSwagger ();
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ("/swagger/v1/swagger.json", "PodNoms.API");
                c.RoutePrefix = "";
            });
            app.UsePodNomsImaging ();
            app.UsePodNomsHangfire (serviceProvider, Configuration, Env.IsProduction ());
            app.UsePodNomsSignalRRoutes ();
            app.UsePodNomsHealthChecks ("/healthcheck");
            app.UsePodNomsApplicationInsights (Configuration.GetSection ("ApplicationInsights"), Env.IsProduction ());
            app.UseSecureHeaders ();

            app.UseMvc (routes => {
                routes.MapRoute (
                    name: "shared",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            JobBootstrapper.BootstrapJobs (Env.IsDevelopment ());
        }
        private static void UpdateDatabase (IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory> ()
                .CreateScope ()) {
                using (var context = serviceScope.ServiceProvider.GetService<PodNomsDbContext> ()) {
                    context.Database.Migrate ();
                }
            }
        }
    }
}
