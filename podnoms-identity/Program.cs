using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using PodNoms.Identity.Data;
using PodNoms.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

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
        options.SetTokenEndpointUris("/connect/token");
        // Enable the password and the refresh token flows.
        options.AllowPasswordFlow()
            .AllowRefreshTokenFlow();
        options.AcceptAnonymousClients();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options => {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PodnomsAuthDbContext>()
    .AddDefaultTokenProviders();
// end setup identity

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
