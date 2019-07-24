using Microsoft.AspNetCore.Builder;

namespace PodNoms.Common.Services.Startup {
    public static class SecurityStartup {
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
                    .StyleSources(s => s.Self())
                    .StyleSources(s => s.UnsafeInline())
                    .StyleSources(s => s.CustomSources("https://podnomscdn.blob.core.windows.net/static/"))
                    .FontSources(s => s.Self())
                    .FontSources(s => s.CustomSources("https://podnomscdn.blob.core.windows.net/static/"))
                    .FormActions(s => s.Self())
                    .FrameAncestors(s => s.Self())
                    .ImageSources(s => s.Self())
                    .ImageSources(s => s.CustomSources("https://podnomscdn.blob.core.windows.net/static/"))
                    .ScriptSources(s => s.Self())
                    .ScriptSources(s => s.UnsafeInline()) //TODO: Look into removing this
                );
            }
            return app;
        }
    }
}
