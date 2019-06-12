using System.Diagnostics;

using FeatureManagementRecipes.Features;

using Microsoft.AspNetCore.Mvc;
using FeatureManagementRecipes.Models;

using Microsoft.FeatureManagement;

namespace FeatureManagementRecipes.Controllers
{
    [Feature(ApplicationFeatureFlags.FeatureA)]
    public class HomeController : Controller
    {
        // Use IFeatureManager if you want features to reflect updates to IConfiguration
        private readonly IFeatureManager _featureManager;
        // Use IFeatureManager if you want features to remain consistent during lifetime of a request
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;

        public HomeController(IFeatureManager featureManager, IFeatureManagerSnapshot featureManagerSnapshot)
        {
            _featureManager = featureManager;
            _featureManagerSnapshot = featureManagerSnapshot;
        }

        public IActionResult Index()
        {
            if (_featureManager.IsEnabled(nameof(ApplicationFeatureFlags.FeatureC)))
            {
                Debug.WriteLine("Debugging enabled, serving Index.");
            }
            return View();
        }

        public IActionResult FeatureNotAvailable()
        {
            if (_featureManager.IsEnabled(nameof(ApplicationFeatureFlags.FeatureC)))
            {
                Debug.WriteLine("DebuggingDebugging enabled, serving FeatureNotAvailable.");
            }
            return View();
        }

        [Feature(ApplicationFeatureFlags.FeatureB)]
        public IActionResult Privacy()
        {
            if (_featureManager.IsEnabled(nameof(ApplicationFeatureFlags.FeatureC)))
            {
                Debug.WriteLine("Debugging enabled, serving Privacy.");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


}
