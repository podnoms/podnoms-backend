using Hangfire.Dashboard;

namespace PodNoms.Common.Auth {
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter {
        public bool Authorize(DashboardContext context) {
            //TODO: Allow all authenticated users to see the Dashboard (potentially dangerous).
            return true;
        }
    }
}