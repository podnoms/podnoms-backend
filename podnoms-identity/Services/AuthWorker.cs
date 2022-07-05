using System.Globalization;
using OpenIddict.Abstractions;
using PodNoms.Identity.Data;

namespace PodNoms.Identity.Services;

public class AuthWorker : IHostedService {
    private readonly IServiceProvider _serviceProvider;

    public AuthWorker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken ctx) {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<PodnomsAuthDbContext>();
        await context.Database.EnsureCreatedAsync(ctx);

        await RegisterApplicationsAsync(scope.ServiceProvider, ctx);
        await RegisterScopesAsync(scope.ServiceProvider);

        static async Task RegisterApplicationsAsync(IServiceProvider provider, CancellationToken ctx) {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            //register main webapp client
            if (await manager.FindByClientIdAsync("webadmin", cancellationToken: ctx) is null) {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor {
                    ClientId = "webadmin",
                    ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                    DisplayName = "PodNoms Web Auth",
                    PostLogoutRedirectUris = {
                        new Uri("https://dev-auth.pdnm.be:4200")
                    },
                    RedirectUris = {
                        new Uri("https://dev-auth.pdnm.be:4200")
                    },
                    Permissions = {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + "podnoms-api-access",
                        OpenIddictConstants.ResponseTypes.Code,
                        OpenIddictConstants.ResponseTypes.Token,
                        OpenIddictConstants.ResponseTypes.IdToken,
                        OpenIddictConstants.ResponseTypes.None
                    },
                    Type = OpenIddictConstants.ClientTypes.Public,
                    Requirements = {
                        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                    },
                }, ctx);
            }

            if (await manager.FindByClientIdAsync("rs_podnoms-api-access", ctx) is null) {
                var descriptor = new OpenIddictApplicationDescriptor {
                    ClientId = "rs_podnoms-api-access",
                    ClientSecret = "arglebarglefooferra",
                    Permissions = {
                        OpenIddictConstants.Permissions.Endpoints.Introspection
                    }
                };
                await manager.CreateAsync(descriptor, ctx);
            }
        }

        static async Task RegisterScopesAsync(IServiceProvider provider) {
            var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync("podnoms-api-access") is null) {
                await manager.CreateAsync(new OpenIddictScopeDescriptor {
                    DisplayName = "PodNoms API access",
                    Name = "podnoms-api-access",
                    Resources = {
                        "rs_podnoms-api-access"
                    }
                });
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
