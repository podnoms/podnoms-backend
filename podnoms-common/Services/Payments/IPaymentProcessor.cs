using System.Threading.Tasks;

namespace PodNoms.Common.Services.Payments {
    public interface IPaymentProcessor {
        Task<StripePaymentResult> ProcessPayment(string orderId, long amount, string description, string idempotencyKey,
            object[] credentials);
    }
}