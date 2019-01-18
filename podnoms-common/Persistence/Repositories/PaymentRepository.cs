using System;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPaymentRepository {
        void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount, string paymentType);
    }

    public class PaymentRepository : GenericRepository<AccountSubscription>, IPaymentRepository {
        public PaymentRepository(PodNomsDbContext context, ILogger<GenericRepository<AccountSubscription>> logger) :
            base(context, logger) { }

        public void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount,
            string paymentType) {
            var newPayment = new AccountSubscription {
                Id = Guid.Parse(orderId),
                Type = paymentType.Equals("Professional")
                    ? AccountSubscriptionType.Professional
                    : AccountSubscriptionType.Advanced,
                AppUser = appUser,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                TransactionId = orderId
            };
            Create(newPayment);
        }
    }
}