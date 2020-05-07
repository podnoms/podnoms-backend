using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class BoilerplateController : BaseController {
        private readonly IRepository<BoilerPlate> _boilerPlateRepository;

        public BoilerplateController(
            IRepository<BoilerPlate> boilerPlateRepository,
            ILogger<BoilerplateController> logger) : base(logger) {
            _boilerPlateRepository = boilerPlateRepository;
        }

        [HttpGet]
        public async Task<ActionResult<BoilerplateViewModel>> Get([FromQuery] string key) {
            var boilerPlate = await _boilerPlateRepository.GetAll()
                .Where(x => x.Key == key)
                .FirstOrDefaultAsync();

            if (boilerPlate == null)
                return NotFound();

            var response = new BoilerplateViewModel {
                Key = key,
                Title = boilerPlate.Title,
                Content = boilerPlate.Content.StartsWith("resource:") ?
                    await ResourceReader.ReadResource($"{boilerPlate.Content.Split(':')[1]}.html", true) :
                    boilerPlate.Content
            };

            return response;
        }
    }
}
