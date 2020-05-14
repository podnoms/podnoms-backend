using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth {
    public class TokenIssuer {
        public static async Task<string> GenerateJwt(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName,
            string[] roles, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings) {
            var response = new {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = await jwtFactory.GenerateEncodedToken(userName, identity, roles),
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds
            };

            return JsonConvert.SerializeObject(response, serializerSettings);
        }
        public static async Task<string> GenerateRawJwt(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName,
            string[] roles, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings) {
            return await jwtFactory.GenerateEncodedToken(userName, identity, roles);
        }
    }
}
