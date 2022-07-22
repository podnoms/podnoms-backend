using System;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using System.Linq;
using System.Collections.Generic;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPaymentRepository : IRepository<AccountSubscription> {
        bool Overlaps(string appUserId, DateTime startDate, DateTime endDate);

        void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount,
            AccountSubscriptionTier paymentType, bool paid, string receipt);

        IEnumerable<AccountSubscription> GetAllValidSubscriptions(string id);
    }

    internal class PaymentRepository : GenericRepository<AccountSubscription>, IPaymentRepository {
        public PaymentRepository(PodNomsDbContext context, ILogger logger) :
            base(context, logger) {
        }

        public bool Overlaps(string appUserId, DateTime startDate, DateTime endDate) {
            var items = GetAll().Any(
                x => x.AppUser.Id == appUserId && x.EndDate >= startDate && x.StartDate <= endDate
            );
            return items;
        }

        public void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount,
            AccountSubscriptionTier paymentType, bool paid, string receipt) {
            var existingSubscription = GetAll()
                .Where(r => r.AppUser == appUser)
                .OrderByDescending(e => e.EndDate)
                .Select(r => r.EndDate)
                .FirstOrDefault();

            var startDate = existingSubscription.Equals(DateTime.MinValue)
                ? DateTime.Now
                : existingSubscription.AddHours(1);

            var newPayment = new AccountSubscription {
                Tier = paymentType,
                Type = AccountSubscriptionType.Stripe,
                AppUser = appUser,
                StartDate = startDate,
                EndDate = startDate.AddMonths(1),
                Amount = paymentAmount,
                WasSuccessful = paid,
                TransactionId = orderId,
                ReceiptURL = receipt
            };
            Create(newPayment);
        }

        public IEnumerable<AccountSubscription> GetAllValidSubscriptions(string userId) {
            var forUser = GetAll()
                .Where(r => r.AppUser.Id == userId)
                .Where(r => DateTime.Now >= r.StartDate)
                .Where(r => DateTime.Now <= r.EndDate);
            return forUser;
        }
    }
}
