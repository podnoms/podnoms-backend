using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;

namespace PodNoms.Common.Services.Payments {
    public class StripePaymentProcessor : IPaymentProcessor {
        public async Task<StripePaymentResult> ProcessPayment(string orderId, long amount, string description,
            string idempotencyKey,
            object[] credentials) {
            StripeConfiguration.SetApiKey("sk_test_VQ9zzpfvaiL98jYYs1P1Wifm");

            var service = new ChargeService();
            var customers = new CustomerService();
            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = credentials[0].ToString(),
                SourceToken = credentials[1].ToString()
            });

            var options = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "eur",
                Metadata = new Dictionary<string, string> {
                    {"OrderId", orderId}
                },
                CustomerId = customer.Id
            };

            var charge = await service.CreateAsync(options);
            return new StripePaymentResult
            {
                Id = charge.Id,
                Paid = charge.Paid,
                Status = charge.Status,
                Amount = charge.Amount,
                ReceiptURL = charge.ReceiptUrl
            };
        }
    }
}