using PodNoms.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PodNoms.Pages.Server.Controllers {
    [Route("api/[controller]")]
    public class PagesController : Controller {
        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> Pages() {
        }
    }
}
