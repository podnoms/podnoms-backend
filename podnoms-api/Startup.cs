using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Settings;
using PodNoms.Api.Persistence;
using PodNoms.Api.Providers;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using PodNoms.Api.Services.Middleware;
using PodNoms.Api.Services.Logging;
using PodNoms.Api.Services.Push.Extensions;
using PodNoms.Services.Services.Startup;

namespace PodNoms.Api {
    public class Startup {
        private const string SecretKey = "QGfaEMNASkNMGLKA3LjgPdkPfFEy3n40";

        private readonly SymmetricSecurityKey
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        private static Mutex mutex = new Mutex();
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration) {
            Configuration = configuration;
            Env = env;

            if (!Env.IsDevelopment()) {
                var logServer = Configuration["StorageSettings:ElasticHost"]?.ToString();
                if (!string.IsNullOrEmpty(logServer)) {
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .MinimumLevel.Debug()
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(logServer)) {
                            MinimumLogEventLevel = LogEventLevel.Verbose,
                            AutoRegisterTemplate = true
                        })
                        .CreateLogger();
                }
            }
        }

        public void ConfigureProductionServices(IServiceCollection services) {
            services.AddDbContext<PodNomsDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            ConfigureServices(services);
            services.AddHangfire(config => {
                config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
            });
        }

        public void ConfigureDevelopmentServices(IServiceCollection services) {
            services.AddDbContext<PodNomsDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("SQLDefaultConnection"));
                // options.EnableSensitiveDataLogging(true);
            });

            ConfigureServices(services);
            services.AddHangfire(config => {
                // config.UseMemoryStorage();
                config.UseSqlServerStorage(Configuration.GetConnectionString("SQLDefaultConnection"));
            });
        }

        public void ConfigureServices(IServiceCollection services) {
            Console.WriteLine($"Configuring services: {Configuration.ToString()}");
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<StorageSettings>(Configuration.GetSection("StorageSettings"));
            services.Configure<HelpersSettings>(Configuration.GetSection("HelpersSettings"));
            services.Configure<FacebookAuthSettings>(Configuration.GetSection("FacebookAuthSettings"));
            services.Configure<ChatSettings>(Configuration.GetSection("ChatSettings"));
            services.Configure<ImageFileStorageSettings>(Configuration.GetSection("ImageFileStorageSettings"));
            services.Configure<AudioFileStorageSettings>(Configuration.GetSection("AudioFileStorageSettings"));
            services.Configure<JwtIssuerOptions>(Configuration.GetSection("JwtIssuerOptions"));
            services.Configure<FormOptions>(options => {
                // options.ValueCountLimit = 10;
                options.ValueLengthLimit = int.MaxValue;
                options.MemoryBufferThreshold = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
            });

            mutex.WaitOne();
            Mapper.Reset();
            services.AddAutoMapper(e => { e.AddProfile(new MappingProvider(Configuration)); });
            mutex.ReleaseMutex();

            services.AddHttpClient("mixcloud", c => {
                c.BaseAddress = new Uri("https://api.mixcloud.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient();
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options => {
                //TODO: Remove this in production, only for testing
                options.ValidFor = TimeSpan.FromDays(28);
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

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
                configureOptions.Events = new JwtBearerEvents() {
                    //Don't need this now we've removed Auth0
                    // OnTokenValidated = AuthenticationMiddleware.OnTokenValidated
                };
                configureOptions.Events.OnMessageReceived = context => {
                    StringValues token;
                    if (context.Request.Path.Value.StartsWith("/hubs/") &&
                        context.Request.Query.TryGetValue("token", out token)) {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(j => {
                j.AddPolicy("ApiUser", policy => policy.RequireClaim(
                    Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            });
            // add identity
            var identityBuilder = services.AddIdentityCore<ApplicationUser>(o => {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
            });
            identityBuilder =
                new IdentityBuilder(identityBuilder.UserType, typeof(IdentityRole), identityBuilder.Services);
            identityBuilder.AddEntityFrameworkStores<PodNomsDbContext>().AddDefaultTokenProviders();
            identityBuilder.AddUserManager<PodNomsUserManager>();

            services.AddMvc(options => {
                    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                    options.OutputFormatters
                        .OfType<StringOutputFormatter>()
                        .Single().SupportedMediaTypes.Add("text/html");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                })
                .AddXmlSerializerFormatters()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info {Title = "PodNoms.API", Version = "v1"});
                c.DocumentFilter<LowercaseDocumentFilter>();
            });

            services.Configure<FormOptions>(x => {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            services.AddCors(options => {
                options.AddPolicy("PodNomsClientPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("http://localhost:4200", "https://*.podnoms.com")
                        .AllowCredentials());
            });

            services.AddPushSubscriptionStore(Configuration);
            services.AddPushNotificationService(Configuration);

            services.AddCors(options => {
                options.AddPolicy("AllowAllPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddPodNomsSignalR();
            services.AddDependencies();
            services.AddPodNomsHangfire(Configuration);
            
            //register the codepages (required for slugify)
            var instance = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(instance);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider, IApplicationLifetime lifetime) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            lifetime.ApplicationStarted.Register(() => {
                var p = new System.Diagnostics.Process {
                    StartInfo = {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = "/usr/bin/play",
                        Arguments =
                            "-n synth -j 3 sin %3 sin %-2 sin %-5 sin %-9 sin %-14 sin %-21 fade h .01 2 1.5 delay 1.3 1 .76 .54 .27 remix - fade h 0 2.7 2.5 norm -1"
                    }
                };
                p.Start();
            });

            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseExceptionHandler(new ExceptionHandlerOptions {
                ExceptionHandler = new JsonExceptionMiddleware(Env).Invoke
            });

            app.UsePodNomsApplicationInsights(
                Env.IsProduction(),
                Configuration.GetSection("ApplicationInsights")
            );

            app.UseCustomDomainRedirect();
            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();

            app.UseCors("AllowAllPolicy");

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PodNoms.API");
                c.RoutePrefix = "";
            });
            app.UsePodNomsHangfire(serviceProvider, Configuration);
            app.UsePodNomsSignalRRoutes();
            
            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}