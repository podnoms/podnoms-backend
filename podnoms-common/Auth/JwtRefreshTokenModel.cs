namespace PodNoms.Common.Auth
{
    public class JwtRefreshTokenModel {
        public JwtRefreshTokenModel() {
        }

        public JwtRefreshTokenModel(string refresh, JwtTokenModel jwt) {
            this.Refresh = refresh;
            this.Jwt = jwt;
        }
        public string Refresh { get; set; }
        public JwtTokenModel Jwt { get; set; }
    }
}
