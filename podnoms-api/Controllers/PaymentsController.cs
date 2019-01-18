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

namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class PaymentsController : BaseAuthController {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _uow;
        private readonly IPaymentProcessor _paymentProcessor;

        public PaymentsController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentsController> logger, IPaymentRepository paymentRepository,
            IUnitOfWork uow,
            IPaymentProcessor paymentProcessor) : base(contextAccessor, userManager,
            logger) {
            _paymentRepository = paymentRepository;
            _uow = uow;
            this._paymentProcessor = paymentProcessor;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentViewModel payment) {
            var orderId = System.Guid.NewGuid().ToString();
            var result = await this._paymentProcessor.ProcessPayment(
                orderId,
                payment.Amount,
                "PodNoms subscription",
                _applicationUser.Id,
                new object[] {"fergal.moran@gmail.com", payment.Token});

            this._paymentRepository.AddPayment(_applicationUser, orderId, payment.Amount, payment.Type);
            await this._uow.CompleteAsync();
            if (result.Paid) {
                return Ok();
            }

            return BadRequest();
        }
    }
}