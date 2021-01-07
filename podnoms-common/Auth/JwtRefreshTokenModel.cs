namespace PodNoms.Common.Auth {
    public class AuthTokenResult {
        public string Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public JwtRefreshTokenModel Auth { get; set; }
    }

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
