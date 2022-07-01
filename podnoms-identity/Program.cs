using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;
using Quartz;
using PodNoms.Identity.Data;
using PodNoms.Identity.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => {
    options.ConfigureHttpsDefaults(httpsOptions => {
        var certPath = Path.Combine("/srv/dev/podnoms/certs/dev-auth.pdnm.be.pem");
        var keyPath = Path.Combine("/srv/dev/podnoms/certs/dev-auth.pdnm.be-key.pem");
        httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
    });
});

builder.Services.AddDbContext<PodnomsAuthDbContext>(options => {
    options
        .UseNpgsql(
            "Host=cluster-master.fergl.ie;Database=podnoms_auth;Username=podnoms_auth;Password=rJ5pqZGKWpNISxI3z8FcWhhbz8UKTfstbr4QOLVquSY=")
        .UseOpenIddict();
});

//setup identity
builder.Services.AddQuartz(options => {
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<AuthWorker>();

builder.Services.AddOpenIddict()
    .AddCore(options => {
        options.UseEntityFrameworkCore()
            .UseDbContext<PodnomsAuthDbContext>();
        options.UseQuartz();
    })
    .AddServer(options => {
        options.SetAuthorizationEndpointUris("/connect/authorize")
            .SetLogoutEndpointUris("/connect/logout")
            .SetIntrospectionEndpointUris("/connect/introspect")
            .SetTokenEndpointUris("/connect/token")
            .SetUserinfoEndpointUris("/connect/userinfo")
            .SetVerificationEndpointUris("/connect/verify");

        options.SetTokenEndpointUris("/connect/token");

        options.AllowAuthorizationCodeFlow()
            .AllowHybridFlow()
            .AllowRefreshTokenFlow();

        options.RegisterScopes(OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.Roles, OpenIddictConstants.Scopes.Roles, "podnoms-api-access");

        options.AcceptAnonymousClients();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options => {
        options.UseLocalServer();
        options.UseAspNetCore();
    });
// .AddClient(options => {
//     options.SetRedirectionEndpointUris(
//         "/signin-local",
//         "/signin-github",
//         "/signin-google",
//         "/signin-reddit",
//         "/signin-twitter");
//
//     options.AddDevelopmentEncryptionCertificate()
//         .AddDevelopmentSigningCertificate();
//
//     options.UseAspNetCore()
//         .EnableStatusCodePagesIntegration()
//         .EnableRedirectionEndpointPassthrough();
//
//     options.UseWebProviders()
//         .AddGitHub(new OpenIddictClientWebIntegrationSettings.GitHub {
//             ClientId = "[client identifier]",
//             ClientSecret = "[client secret]",
//             RedirectUri = new Uri("https://dev-auth.pdnm.be:5003/signin-github", UriKind.Absolute)
//         })
//         .AddGoogle(new OpenIddictClientWebIntegrationSettings.Google {
//             ClientId = "[client identifier]",
//             ClientSecret = "[client secret]",
//             RedirectUri = new Uri("https://dev-auth.pdnm.be:5003/signin-google", UriKind.Absolute),
//             Scopes = {OpenIddictConstants.Permissions.Scopes.Profile}
//         });
// });

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAllOrigins",
        corsPolicyBuilder => {
            corsPolicyBuilder
                .AllowCredentials()
                .WithOrigins(
                    "https://dev-auth.pdnm.be:4200"
                )
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PodnomsAuthDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// end setup identity

builder.Services.AddRazorPages();
// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
    endpoints.MapDefaultControllerRoute();
    endpoints.MapRazorPages();
});

app.Run();
