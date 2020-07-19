using System.Text.Json.Serialization;

namespace PodNoms.Common.Data.ViewModels.Remote {
    public class PatreonAuthTokenResponseModel {

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
       
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }      
        
        [JsonPropertyName("_full_name")]
        public string FullName { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
    }
}
