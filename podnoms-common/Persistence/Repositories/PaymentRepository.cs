using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PodNoms.Data.Extensions;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IPaymentRepository : IRepository<AccountSubscription> {
        bool Overlaps(string appUserId, DateTime startDate, DateTime endDate);
        void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount, string paymentType, bool paid, string receipt);
        IEnumerable<AccountSubscription> GetAllValidSubscriptions(string id);
    }

    public class PaymentRepository : GenericRepository<AccountSubscription>, IPaymentRepository {
        public PaymentRepository(PodNomsDbContext context, ILogger<GenericRepository<AccountSubscription>> logger) :
            base(context, logger) { }

        public bool Overlaps(string appUserId, DateTime startDate, DateTime endDate) {
            var items = GetAll().Any(
                x => x.AppUser.Id == appUserId && x.EndDate >= startDate && x.StartDate <= endDate
            );
            return items;
        }

        public void AddPayment(ApplicationUser appUser, string orderId, long paymentAmount,
            string paymentType, bool paid, string receipt) {
            var existingSubscription = GetAll()
                .Where(r => r.AppUser == appUser)
                .OrderByDescending(e => e.EndDate)
                .Select(r => r.EndDate)
                .FirstOrDefault();

            var startDate = existingSubscription.Equals(DateTime.MinValue)
                ? DateTime.Now
                : existingSubscription.AddHours(1);

            var newPayment = new AccountSubscription
            {
                Type = paymentType.Equals("professional")
                    ? AccountSubscriptionType.Professional
                    : AccountSubscriptionType.Advanced,
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

        public IEnumerable<AccountSubscription> GetAllValidSubscriptions(string id) {

            var forUser = GetAll().Where(r => r.AppUser.Id == id).Where(r => DateTime.Now >= r.StartDate)
                    .Where(r => DateTime.Now <= r.EndDate);
            return forUser;
        }
    }
}