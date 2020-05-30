namespace PodNoms.Common.Auth {
    public class JwtRefreshTokenModel {
        public JwtRefreshTokenModel(string refresh, JwtTokenModel jwt) {
            this.Refresh = refresh;
            this.Jwt = jwt;
        }
        public string Refresh { get; set; }
        public JwtTokenModel Jwt { get; set; }
    }
    public class JwtTokenModel {
        public JwtTokenModel(string Id, string token, int expiresIn) {
            this.Id = Id;
            this.Token = token;
            this.ExpiresIn = expiresIn;
        }
        public string Id { get; set; }
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
    }
}
