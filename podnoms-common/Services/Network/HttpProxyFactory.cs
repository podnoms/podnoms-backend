using System.Net.Http;

namespace PodNoms.Common.Services.Network {
    public static class HttpProxyFactory {
        public static HttpClientHandler GetZapProxy() {
            return new HttpClientHandler {
                Proxy = new System.Net.WebProxy("https://localhost:9327"),
                UseProxy = true,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
        }
    }
}
