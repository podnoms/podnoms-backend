using System;
using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Common.Auth.ApiKeys {
    public class ForbiddenProblemDetails : ProblemDetails {
        public ForbiddenProblemDetails(string details = null) {
            Title = "Forbidden";
            Detail = details;
            Status = 403;
            Type = "https://httpstatuses.com/403";
        }
    }
}