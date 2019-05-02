using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Extensions;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models;
using Zxcvbn;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class UtilityController : BaseAuthController {
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PodNomsDbContext _context;

        public UtilityController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
                        IOptions<AppSettings> appSettings, PodNomsDbContext context,
                        ILogger<UtilityController> logger, IConfiguration config,
                        IHttpClientFactory httpClientFactory)
                                    : base(contextAccessor, userManager, logger) {
            _appSettings = appSettings.Value;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet("checkdupes")]
        public async Task<ActionResult<CheckValueResult>> CheckForDupes(string table, string field, string value, string narrative = "Title") {
            return await Task.Run(() => {
                try {
                    var p = new Dictionary<string, object>();
                    p.Add("field", value);
                    var sql = $"SELECT {field} AS Value, {narrative} AS ResponseMessage FROM {table} WHERE {field} = @field";
                    var result = _context.CollectionFromSql(sql, p).FirstOrDefault();
                    if (result != null) {
                        return Ok(new CheckValueResult
                        {
                            IsValid = false,
                            Value = value,
                            ResponseMessage = result.ResponseMessage
                        });
                    }
                } catch (Exception ex) {
                    _logger.LogError($"Error checking for dupes\n{ex.Message}\n\tTable: {table}\n\tField: {field}\n\tValue: {value}\n\tNarrative: {narrative}");
                }
                return Ok(new CheckValueResult
                {
                    Value = value,
                    IsValid = true,
                    ResponseMessage = "No duplicates found"
                });
            });
        }
        [HttpPost("checkdomain")]
        public async Task<ActionResult<bool>> CheckHostName([FromBody]CheckHostNameViewModel request) {
            try {
                _logger.LogInformation($"Checking domain: {request.HostName}");

                var dnsRequest = new ClientRequest("8.8.8.8");
                dnsRequest.Questions.Add(new Question(Domain.FromString(request.HostName), RecordType.CNAME));
                dnsRequest.RecursionDesired = true;

                var response = await dnsRequest.Resolve();

                var result = response.AnswerRecords
                   .Where(r => r.Type == RecordType.CNAME)
                   .Cast<CanonicalNameResourceRecord>()
                   .Select(r => r.CanonicalDomainName)
                   .FirstOrDefault();
                return Ok(result?.CompareTo(new Domain(new Uri(_appSettings.RssUrl).Host)) == 0);
            } catch (Exception ex) {
                _logger.LogError($"Error checking domain {request}");
                _logger.LogError(ex.Message);
            }
            return Ok(false);
        }
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
        public ActionResult<string> GetTemporaryPodcastImage() {
            var image = ImageUtils.GetTemporaryImage("podcast", 16);
            return $"\"{_config.GetSection("StorageSettings")["CdnUrl"]}static/images/{image}\"";
        }

        [HttpGet("filesize")]
        public async Task<long> GetRemoteFileSize([FromQuery]string url) {
            using (var client = _httpClientFactory.CreateClient()) {
                return await client.GetContentSizeAsync(url);
            }
        }
    }
}
