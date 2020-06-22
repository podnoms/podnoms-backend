namespace PodNoms.Common.Auth {
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
