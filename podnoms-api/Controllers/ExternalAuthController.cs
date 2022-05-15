using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Persistence;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]/[action]")]
    [EnableCors("PodNomsClientPolicy")]
    public class ExternalAuthController : BaseController {
        //TODO: Refactor these somewhere better
        public static ClientSecrets secrets = new ClientSecrets() {
            ClientSecret = "wPXd9Sw9Z04bnGrobkZoZoGz"
        };

        // Configuration that you probably don't need to change.
        static public string APP_NAME = "PodNoms Web API";

        static public string[] SCOPES = {
            PlusService.Scope.PlusLogin,
            PlusService.Scope.UserinfoEmail,
            PlusService.Scope.UserinfoProfile
        };
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FacebookAuthSettings _fbAuthSettings;
        private readonly IJwtFactory _jwtFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRepoAccessor _repoAccessor;
        private readonly JwtIssuerOptions _jwtOptions;
        private static readonly HttpClient Client = new HttpClient();

        public ExternalAuthController(IOptions<FacebookAuthSettings> fbAuthSettingsAccessor, UserManager<ApplicationUser> userManager,
            IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions,
            IHttpContextAccessor contextAccessor,
            IRepoAccessor repoAccessor,
            ILogger<ExternalAuthController> logger) : base(logger) {
            _fbAuthSettings = fbAuthSettingsAccessor.Value;
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _contextAccessor = contextAccessor;
            _repoAccessor = repoAccessor;
            _jwtOptions = jwtOptions.Value;
        }
        // POST api/externalauth/google
        [HttpPost]
        public async Task<ActionResult<JwtRefreshTokenModel>> Google([FromBody] SocialAuthViewModel model) {
            //1. Validate access token
            //2. Generate JWT
            //3. Update details
            try {
                var payload = await GoogleJsonWebSignature.ValidateAsync(model.AccessToken);
                var tokenAndRefresh = await _processUserDetails(new FacebookUserData {
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    Name = payload.Name,
                    Picture = new FacebookPictureData {
                        Data = new FacebookPicture {
                            Url = payload.Picture
                        }
                    }
                });
                Response.Cookies.Append(
                    "SESSIONID",
                    tokenAndRefresh.Jwt.Token,
                    new CookieOptions() {
                        Path = "/",
                        HttpOnly = false,
                        Secure = false
                    }
                );
                return Ok(tokenAndRefresh);
            } catch (InvalidOperationException e) {
                _logger.LogError(e.Message);
                return BadRequest(Errors.AddErrorToModelState("login_failure", e.Message, ModelState));
            } catch (Exception ex) {
                _logger.LogError($"Error logging in: {ex.Message}");
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid google token.", ModelState));
            }
        }

        // POST api/externalauth/facebook
        [HttpPost]
        public async Task<ActionResult<JwtRefreshTokenModel>> Facebook([FromBody] SocialAuthViewModel model) {
            // 1.generate an app access token
            var appAccessTokenResponse = await Client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_fbAuthSettings.AppId}&client_secret={_fbAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await Client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid) {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid facebook token.", ModelState));
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await Client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,gender,locale,birthday,picture&access_token={model.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            var tokenAndRefresh = await _processUserDetails(userInfo);

            Response.Cookies.Append(
                "SESSIONID",
                tokenAndRefresh.Jwt.Token,
                new CookieOptions() {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false
                }
            );

            return Ok(tokenAndRefresh);
        }
        private async Task<JwtRefreshTokenModel> _processUserDetails(FacebookUserData userInfo) {
            // 4. ready to create the local user account (if necessary) and jwt
            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            if (user is null) {
                var appUser = new ApplicationUser {
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    FacebookId = userInfo.Id,
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    PictureUrl = userInfo.Picture.Data.Url
                };
                var result = await _userManager.CreateAsync(appUser, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));
                if (!result.Succeeded) {
                    throw new InvalidOperationException(
                        ModelState.ToString()
                    );
                }
            } else {
                user.PictureUrl = userInfo.Picture.Data.Url;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) {
                    throw new InvalidOperationException(
                        ModelState.ToString()
                    );
                }
            }

            // generate the jwt for the local user...
            var localUser = await _userManager.FindByNameAsync(userInfo.Email);
            if (localUser is null) {
                throw new InvalidOperationException(
                    ModelState.ToString()
                );
            }
            var roles = await _userManager.GetRolesAsync(localUser);
            var jwt = await TokenIssuer.GenerateJwt(
                _jwtFactory.GenerateClaimsIdentity(localUser.UserName, localUser.Id),
                _jwtFactory,
                localUser.UserName,
                roles.ToArray<string>(),
                _jwtOptions,
                new JsonSerializerSettings { Formatting = Formatting.Indented });

            var refresh = TokenIssuer.GenerateRefreshToken(128);
            user.AddRefreshToken(
                refresh,
                _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());

            await _repoAccessor.CompleteAsync();
            return new JwtRefreshTokenModel(refresh, jwt);
        }
    }
}
