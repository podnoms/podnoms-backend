using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Extensions;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Remote;
using PodNoms.Common.Persistence.Extensions;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Models;
using PodNoms.Common.Persistence;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class UtilityController : BaseAuthController {
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PodNomsDbContext _context;
        private readonly IRepoAccessor _repo;

        public UtilityController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings, PodNomsDbContext context, IRepoAccessor repo,
            ILogger logger, IConfiguration config,
            IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory)
            : base(contextAccessor, userManager, logger) {
            _appSettings = appSettings.Value;
            _config = config;
            _env = env;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _repo = repo;
        }

        [HttpGet("checkdupes")]
        public async Task<ActionResult<CheckValueResult>> CheckForDupes(string table, string field, string value,
            string narrative = "Title") {
            return await Task.Run(() => {
                try {
                    var p = new Dictionary<string, object> {{"field", value}};
                    var sql =
                        $"SELECT {field} AS Value, {narrative} AS ResponseMessage FROM {table} WHERE {field} = @field";
                    var result = _context.CollectionFromSql(sql, p).FirstOrDefault();
                    if (result != null) {
                        return Ok(new CheckValueResult {
                            IsValid = false,
                            Value = value,
                            ResponseMessage = result.ResponseMessage
                        });
                    }
                } catch (Exception ex) {
                    _logger.LogError(
                        $"Error checking for dupes\n{ex.Message}\n\tTable: {table}\n\tField: {field}\n\tValue: {value}\n\tNarrative: {narrative}");
                }

                return Ok(new CheckValueResult {
                    Value = value,
                    IsValid = true,
                    ResponseMessage = "No duplicates found"
                });
            });
        }

        [HttpPost("checkdomain")]
        public async Task<ActionResult<bool>> CheckHostName([FromBody] CheckHostNameViewModel request) {
            if (_env.IsDevelopment() && false) {
                return Ok(true);
            }

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
                return Ok(result?.Equals(new Domain(request.RequiredDomain)) ?? false);
            } catch (Exception ex) {
                _logger.LogError($"Error checking domain {request}");
                _logger.LogError(ex.Message);
            }

            return Ok(false);
        }

        [AllowAnonymous]
        [HttpPost("checkpassword")]
        public async Task<ActionResult<int>> CheckPasswordStrength([FromBody] string pwd) {
            if (string.IsNullOrEmpty(pwd))
                return BadRequest();

            var r = Zxcvbn.Core.EvaluatePassword(pwd);
            return await Task.FromResult(r.Score);
        }

        [HttpGet("temppodcastimage")]
        public ActionResult<string> GetTemporaryPodcastImage() {
            var image = ImageUtils.GetTemporaryImage("podcast", 16);
            return $"\"{_config.GetSection("StorageSettings")["CdnUrl"]}static/images/{image}\"";
        }

        [HttpGet("filesize")]
        public async Task<long> GetRemoteFileSize([FromQuery] string url) {
            using (var client = _httpClientFactory.CreateClient()) {
                return await client.GetContentSizeAsync(url);
            }
        }

        [HttpGet("clearhangfire")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "catastrophic-api-calls-allowed")]
        public async Task<ActionResult> ClearHangfireTables() {
            using (var connection = new SqlConnection(_config["ConnectionStrings:JobSchedulerConnection"])) {
                SqlCommand cmd = new SqlCommand("maintenance.SP_ResetHangfire", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                await cmd.ExecuteNonQueryAsync();
                connection.Close();
            }

            return Ok();
        }

        [HttpGet("randomimage")]
        public async Task<ActionResult> GetRandomImage() {
            var image = await ImageUtils.GetRandomImageAsBase64(_httpClientFactory);
            if (!string.IsNullOrEmpty(image)) {
                return Content(image, "text/plain", Encoding.UTF8);
            }

            return new NotFoundResult();
        }

        [HttpGet("opml/{userSlug}")]
        [Produces("application/xml")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOpml(string userSlug) {
            var user = await _userManager.FindBySlugAsync(userSlug);
            if (user == null) {
                return NotFound();
            }

            var result = await user.GetOpmlFeed(
                _repo,
                _appSettings.RssUrl,
                _appSettings.SiteUrl);

            return Content(result, "application/xml", Encoding.UTF8);
        }
    }
}
