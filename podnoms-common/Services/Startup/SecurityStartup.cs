using Microsoft.AspNetCore.Builder;

namespace PodNoms.Common.Services.Startup {
    public static class SecurityStartup {
        public static IApplicationBuilder UseSecureHeaders(
            this IApplicationBuilder app) {
            app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                .StyleSources(s => s.Self())
                .StyleSources(s => s.UnsafeInline())
                .FontSources(s => s.Self())
                .FormActions(s => s.Self())
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ScriptSources(s => s.Self())
            );

            return app;
        }
    }
}