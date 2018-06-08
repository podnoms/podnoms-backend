using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Utils;
using Zxcvbn;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class UtilityController : BaseAuthController {
        private readonly IConfiguration _config;

        public UtilityController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                    ILogger<UtilityController> logger, IConfiguration config) : base(contextAccessor, userManager, logger) {
            this._config = config;
        }

        // [AllowAnonymous]
        // [HttpPost("checkdomain")]
        // public async Task<ActionResult<bool>> CheckHostName([FromBody]string hostname) {
        //     ClientRequest request = new ClientRequest("8.8.8.8");
        //     request.Questions.Add(new Question(Domain.FromString(hostname), RecordType.CNAME));
        //     request.RecursionDesired = true;

        //     var response = await request.Resolve();

        //     var ips = response.AnswerRecords
        //         .OfType<CanonicalNameResourceRecord>()
        //         .Where(r => r.Type == RecordType.CNAME)
        //         .Where(r => r.CanonicalDomainName.ToString().Equals("rss.podnoms.com"))
        //         .ToList();

        //     return Ok(ips.Count != 0);
        // }
        [AllowAnonymous]
        [HttpPost("checkpassword")]
        public async Task<ActionResult<int>> CheckPasswordStrength([FromBody]string pwd) {
            if (string.IsNullOrEmpty(pwd))
                return BadRequest();
                
            var z = new Zxcvbn.Zxcvbn();
            var r = z.EvaluatePassword(pwd);
            return await Task.FromResult<int>(new Zxcvbn.Zxcvbn().EvaluatePassword(pwd).Score);
        }

        [HttpGet("temppodcastimage")]
        public ActionResult<string> GetTemporaryPodcastImage(){
            var image = ImageUtils.GetTemporaryImage("podcast", 16);
            return $"\"{this._config.GetSection("StorageSettings")["CdnUrl"]}static/images/{image}\"";
        }
    }
}
