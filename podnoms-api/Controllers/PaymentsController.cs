using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Services.Payments;
using PodNoms.Data.Models;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PodNoms.Common.Services;
using PodNoms.Common.Data.ViewModels.Resources;

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class PaymentsController : BaseAuthController {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDonationRepository _donationRepository;
        private readonly IMailSender _mailSender;
        private readonly IUnitOfWork _uow;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly HttpClient _httpClient;

        public PaymentsController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentsController> logger,
            IPaymentRepository paymentRepository,
            IDonationRepository donationRepository,
            IMailSender mailSender,
            IUnitOfWork uow,
            IPaymentProcessor paymentProcessor,
            IHttpClientFactory httpClientFactory) : base(contextAccessor, userManager,
            logger) {
            _paymentRepository = paymentRepository;
            this._donationRepository = donationRepository;
            this._mailSender = mailSender;
            _uow = uow;
            this._paymentProcessor = paymentProcessor;
            this._httpClient = httpClientFactory.CreateClient("StripeInvoices");
        }
        [HttpGet]
        public async Task<ActionResult<List<PaymentLogViewModel>>> GetPayments() {
            var payments = await _paymentRepository.GetAll()
                .Where(r => r.AppUser.Id == _applicationUser.Id)
                .OrderByDescending(e => e.CreateDate)
                .Select(e => new PaymentLogViewModel {
                    Id = e.Id.ToString(),
                    TransactionId = e.TransactionId,
                    Amount = e.Amount,
                    WasSuccessful = e.WasSuccessful,
                    Type = e.Type,
                    CreateDate = e.CreateDate,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    ReceiptURL = e.ReceiptURL
                })
                .ToListAsync();

            return Ok(payments);
        }
        [HttpPost]
        public async Task<ActionResult<StripePaymentResult>> ProcessPayment([FromBody] PaymentViewModel payment) {
            var orderId = System.Guid.NewGuid().ToString();
            var result = await this._paymentProcessor.ProcessPayment(
                orderId,
                payment.Amount,
                "PodNoms subscription",
                _applicationUser.Id,
                new object[] { _applicationUser.Email, payment.Token });
            if (payment.Type == AccountSubscriptionType.Free && result.Paid) {
                this._donationRepository.AddOrUpdate(new Donation {
                    AppUser = _applicationUser,
                    Amount = payment.Amount,
                });
                await this._mailSender.SendEmailAsync(
                    _applicationUser.Email,
                    "Thank you so much!!!",
                    new MailDropin {
                        username = _applicationUser.GetBestGuessName(),
                        title = "New Notification",
                        message = $"<p>Hey Thanks SO much for your donation {_applicationUser.GetBestGuessName()}, it really is appreciated!</p><p>Kindest regards,</p><p>Fergal</p>",
                    });
            } else {
                this._paymentRepository.AddPayment(
                    _applicationUser, orderId,
                    payment.Amount, // convert from cents to 
                    payment.Type,
                    result.Paid,
                    result.ReceiptURL);
            }
            await this._uow.CompleteAsync();
            if (result.Paid) {
                return Ok(result);
            } else {
                return StatusCode(StatusCodes.Status402PaymentRequired);
            }
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> DownloadInvoice(string invoiceId) {
            var payment = this._paymentRepository
                .GetAll()
                .Where(r => r.Id == Guid.Parse(invoiceId))
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(payment?.ReceiptURL)) {
                using (var result = await this._httpClient.GetAsync(payment.ReceiptURL)) {
                    if (result.IsSuccessStatusCode) {
                        var bytes = await result.Content.ReadAsByteArrayAsync();
                        var response = File(bytes, "text/html", $"PodNoms Invoice - {payment.StartDate.ToShortDateString()}");
                        return Ok(response);
                    } else {
                        _logger.LogError($"Error proxying invoice: {result.StatusCode} - {result.ReasonPhrase}");
                    }
                }
            }
            return NotFound();
        }
        [AllowAnonymous]
        [HttpGet("pricingtiers")]
        public ActionResult<List<PricingTierViewModel>> GetPricingTiers() {
            return Ok(new PricingTierController().PricingTiers);
        }
        [AllowAnonymous]
        [HttpGet("pricingtier/{tierType}")]
        public ActionResult<PricingTierController> GetPricingTier(string tierType) {
            if (Enum.TryParse(tierType, out AccountSubscriptionType type)) {
                var tiers = new PricingTierController().PricingTiers.Where(r => r.Type == type);
                if (tiers.Count() != 0) {
                    return Ok(tiers.FirstOrDefault());
                }
            }
            return NotFound();
        }
    }
}
