using PodNoms.Identity.Data;

namespace PodNoms.Identity.Services;

public class AuthWorker : IHostedService {
    private readonly IServiceProvider _serviceProvider;

    public AuthWorker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken) {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<PodnomsAuthDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
