using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Common;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;
using reCAPTCHA.AspNetCore;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [EnableCors("PodNomsClientPolicy")]
    public class AuthController : BaseController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly IRecaptchaService _recaptcha;
        private readonly IMailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        public RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppSettings _appSettings;
        private readonly JwtIssuerOptions _jwtOptions;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor contextAccessor,
            IJwtFactory jwtFactory,
            IRecaptchaService recaptcha,
            IOptions<JwtIssuerOptions> jwtOptions,
            IOptions<AppSettings> appSettings,
            IMailSender mailSender,
            IUnitOfWork unitOfWork,
            ILogger<AuthController> logger) : base(logger) {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _recaptcha = recaptcha;
            _emailSender = mailSender;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _contextAccessor = contextAccessor;
            _appSettings = appSettings.Value;
            _jwtOptions = jwtOptions.Value;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<JwtRefreshTokenModel>> Post([FromBody] CredentialsViewModel credentials) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            var user = await _userManager.FindByEmailAsync(credentials.UserName);
            if (user is null) {
                user = await _userManager.FindByNameAsync(credentials.UserName);
            }
            if (user is null) {
                return Unauthorized();
            }
            var roles = await _userManager.GetRolesAsync(user);

            if (identity is null) {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }

            var (token, refresh) = await _getTokenAndRefresh(identity, credentials.UserName, roles.ToArray<string>(), user);
            Response.Cookies.Append(
                "SESSIONID",
                token,
                new CookieOptions() {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false
                }
            );
            return Ok(refresh);
        }
        private async Task<(string, JwtRefreshTokenModel)> _getTokenAndRefresh(ClaimsIdentity identity, string userName, string[] roles, ApplicationUser user) {
            var jwt = await TokenIssuer.GenerateJwt(
                identity,
                _jwtFactory,
                userName,
                roles,
                _jwtOptions,
                new JsonSerializerSettings { Formatting = Formatting.Indented }
            );
            var refresh = TokenIssuer.GenerateRefreshToken(128);
            user.AddRefreshToken(
                refresh,
                _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());

            await _unitOfWork.CompleteAsync();
            return (jwt.Token, new JwtRefreshTokenModel(refresh, jwt));
        }
        [HttpPost("refreshtoken")]
        public async Task<ActionResult<JwtRefreshTokenModel>> RefreshToken([FromBody] ExchangeRefreshTokenRequest request) {
            var userName = _jwtFactory.DecodeToken(request.AccessToken);
            if (userName == null) {
                return BadRequest("No user with that auth token");
            }
            var user = await _userManager
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(
                    r => r.UserName == userName &&
                    r.RefreshTokens.Any(p =>
                        p.Token.Equals(request.RefreshToken) &&
                        p.CreateDate >= System.DateTime.Now.AddDays(-28)));

            if (user == null) {
                return BadRequest("Cannot find your refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var identity = _jwtFactory.GenerateClaimsIdentity(userName, user.Id);

            var token = await _getTokenAndRefresh(identity, userName, roles.ToArray<string>(), user);
            return Ok(token);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password) {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify is null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password)) {
                await _userManager.UpdateAsync(userToVerify);
                var identity = _jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id);
                return await Task.FromResult(identity);
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }

        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user is null) {
                    _logger.LogWarning($"Password reset requested for {model.Email}");
                    return Ok(model);
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = $"{_appSettings.SiteUrl}/auth/reset?token={WebUtility.UrlEncode(code)}&email={WebUtility.UrlEncode(user.Email)}";
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "PodNoms Reset Password Request",
                    new MailDropin {
                        username = user.GetBestGuessName(),
                        title = "Password Rest Request",
                        message = @"Someone told us you forgot your password?<br />
                                    <span style='color: #a8bf6f; font-size: 14px; line-height: 21px;'>Don't worry, it happens.</span>",
                        buttonaction = callbackUrl,
                        buttonmessage = "Reset Password"
                    });
                return Ok(model);
            }
            return BadRequest(model);
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordViewModel model) {
            if (!ModelState.IsValid) {
                return BadRequest("Unable to reset your password at this time");
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user is null) {
                return BadRequest("Unable to reset your password at this time");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded) {
                return Ok(model);
            }
            return BadRequest();
        }

        [HttpGet("addrole/{role}")]
        public async Task<IActionResult> AddRole(string role) {
            if (!await _roleManager.RoleExistsAsync(role)) {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            return Json(_roleManager.Roles);
        }

        [HttpGet("addusertorole/{role}/{slug}")]
        public async Task<IActionResult> AddUserToRole(string role, string slug) {
            _logger.LogDebug($"Adding {slug} to {role}");
            if (!await _roleManager.RoleExistsAsync(role)) {
                _logger.LogDebug("Creating role");
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            ApplicationUser user = await _userManager.FindBySlugAsync(slug);
            if (user == null) {
                _logger.LogError("Unable to find user");
                return NotFound();
            }
            if (!User.IsInRole(role)) {
                await _userManager.AddToRoleAsync(user, role);
            }
            return Json(await _userManager.GetRolesAsync(user));
        }
        [HttpPost("validaterecaptha")]
        public async Task<ActionResult<TokenValidationViewModel>> ValidateCaptcha([FromBody] TokenValidationViewModel model) {
            var recaptcha = await _recaptcha.Validate(model.Token);
            if (!recaptcha.success) {
                model.IsValid = false;
                BadRequest(model);
            }
            model.IsValid = true;
            return Ok(model);
        }
    }
}
