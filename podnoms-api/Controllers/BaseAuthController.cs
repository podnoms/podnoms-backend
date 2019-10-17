﻿using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Auth;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Authorize]
    public abstract class BaseAuthController : BaseController {
        private readonly ClaimsPrincipal _caller;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly string _userId;
        protected readonly ApplicationUser _applicationUser;
        //TODO: IMPORTANT
        //This should be the IHttpContextAccessor 
        //and should be used like _httpContextAccessor.HttpContext AT POINT OF USE
        protected readonly IHttpContextAccessor _httpContextAccessor;
        //TO BE CLEAR... delete the below and use the above instead ^^ 
        protected readonly HttpContext _httpContext;
        public BaseAuthController(IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger logger) : base(logger) {
            _caller = contextAccessor.HttpContext.User;
            _userManager = userManager;
            _httpContext = contextAccessor.HttpContext;
            _httpContextAccessor = contextAccessor;
            try {
                if (!_caller.Identity.IsAuthenticated) {
                    return;
                }
                var claim = _caller.Claims.Single(c => c.Type == "id");
                if (claim != null) {
                    _userId = _caller.Claims.Single(c => c.Type == "id")?.Value;
                    if (_userId != null) {
                        _applicationUser = userManager.FindByIdAsync(_userId).Result;
                    }
                }
            } catch (System.InvalidOperationException ex) {
                _logger.LogError($"Error constructing BaseAuthController: \n{ex.Message}");
            }
        }
    }
}
