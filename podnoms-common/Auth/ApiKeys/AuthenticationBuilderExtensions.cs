using System;
using Microsoft.AspNetCore.Authentication;

namespace PodNoms.Common.Auth.ApiKeys {

    public static class AuthenticationBuilderExtensions {
        public static AuthenticationBuilder AddApiKeySupport(
                this AuthenticationBuilder authenticationBuilder, 
                Action<ApiKeyAuthenticationOptions> options) {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
        }
    }
}
