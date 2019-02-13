using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace PodNoms.Api.Controllers {
    public abstract class BaseController : Controller {
        protected readonly ILogger _logger;
        public BaseController(ILogger logger) {
            _logger = logger;
        }
    }
}
