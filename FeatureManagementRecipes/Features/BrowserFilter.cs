using System.Linq;

using FeatureManagementRecipes.Settings;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace FeatureManagementRecipes.Features
{
    [FilterAlias("Browser")]
    public class BrowserFilter : IFeatureFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static string _hardCodedBrowserName = "Chrome";

        public BrowserFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Evaluate(FeatureFilterEvaluationContext context)
        {
            BrowserFilterSettings settings = context.Parameters.Get<BrowserFilterSettings>();

            return settings == null || context.FeatureName != nameof(ApplicationFeatureFlags.FeatureA) || settings.AllowedBrowsers.Contains(_hardCodedBrowserName);
        }
    }
}