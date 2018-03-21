using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers {
    [Route("/sse")]
    public class ServerEventsController : Controller {
        public string Get(){
            return "Hello Sailor!";
        }
    }
}
