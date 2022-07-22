using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;

namespace PodNoms.Common.Services.Jobs {
    public abstract class BlobEnumerateJob : AbstractHostedJob {
        protected BlobEnumerateJob(ILogger<BlobEnumerateJob> logger) : base(logger) {
        }

        protected async IAsyncEnumerable<T> GetBlobs<T>(
                CloudBlobContainer container,
                int? maxResultsPerQuery = null) where T : ICloudBlob {

            BlobContinuationToken continuationToken = null;

            do {
                var response = await container.ListBlobsSegmentedAsync(
                    string.Empty,
                    true,
                    BlobListingDetails.None,
                    maxResultsPerQuery,
                    continuationToken,
                    null,
                    null);
                continuationToken = response.ContinuationToken;
                foreach (var blob in response.Results.OfType<T>()) {
                    yield return blob;
                }
            } while (continuationToken != null);
        }
    }
}
