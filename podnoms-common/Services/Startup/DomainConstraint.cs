using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
namespace PodNoms.Common.Services.Startup {
    public class DomainConstraint : IRouteConstraint {

        public string _value { get; private set; }
        public DomainConstraint(string value) {
            _value = value;
        }

        public bool Match(HttpContext httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            string hostURL = httpContext.Request.Host.ToString();
            if (hostURL == _value) {
                return true;
            }
            //}
            return false;
            //return hostURL.IndexOf(_value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) {
            throw new NotImplementedException();
        }
    }
    public class DomainConstraintRouteAttribute : RouteAttribute {
        public DomainConstraintRouteAttribute(string template, string sitePermitted)
            : base(template) {
            SitePermitted = sitePermitted;
        }

        public RouteValueDictionary Constraints {
            get {
                var constraints = new RouteValueDictionary();
                constraints.Add("host", new DomainConstraint(SitePermitted));
                return constraints;
            }
        }

        public string SitePermitted {
            get;
            private set;
        }
    }
}