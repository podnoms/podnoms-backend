using Microsoft.AspNetCore.Authentication;


namespace PodNoms.Common.Auth.ApiKeys {
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions {
        public static string Scheme => "PodNomsApiKey";
        public static string DefaultScheme => "PodNomsApiKey";
        public static string AuthenticationType => "PodNomsApiKey";
    }
}
