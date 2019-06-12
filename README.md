## Microsoft.FeatureManagement and Microsoft.FeatureManagement.AspNetCore

The Azure team has put out a feature management library that looks very promising. I should point out early that even though this is from the Azure team and is meant to work with Azure Feature Manager the library itself is totally agnostic of Azure and only has a dependency on Microsoft.Extensions.Configuration as source of config. The Microsoft.FeatureManagement can be used with any .net app and Microsoft.FeatureManagement.AspNetCoreMicrosoft.FeatureManagement.AspNetCore specifically with asp.net core. The library is available on nuget but as a prerelease at the moment. The Azure Feature Manager is also in preview and hence free to try, but I will not be covering that in this article.

### So what's all the fuss about?

So why do we need a library for feature management, is it not easy to have create custom code to support feature flags? The short answer is yes, but this library offers more than just the basic, let's get into it.

The main concepts supported by this library are:

- Feature flags with simple binary states.
- Feature management that handles the life cycle of feature flag. For instance you may want changes to your config to immediately reflect or stay consistent per request at least.
- Filters to allow you to add logic to how a feature flag is evaluated.

### Configuration

The library depends on Microsoft.Extensions.Configuration and supported config source. In my examples I will make use of an appsettings.json config.

To configure basic feature flags this can be done as:

```JavaScript
   "FeatureManagement": {
    "FeatureA": true, // On feature
    "FeatureB": false // Off feature
  }
```

To make use of more complex scenarios you make use of filters to further specify how a feature flag is evaluated.

```JavaScript
  "FeatureManagement": {
    "FeatureC": {
      "EnabledFor": [
        {
          "Name": "AlwaysOn"
        }
      ]
    },
    "FeatureD": {
      "EnabledFor": []
    },
    "FeatureE": {
      "EnabledFor": [
        {
          "Name": "TimeWindow",
          "Parameters": {
            "Start": "01 May 2019 13:59:59 GMT",
            "End": "01 July 2019 00:00:00 GMT"
          }
        }
      ]
    },
    "FeatureF": {
      "EnabledFor": [
        {
          "Name": "Browser",
          "Parameters": {
            "AllowedBrowsers": [ "Chrome", "Edge" ]
          }
        }
      ]
    },
    "FeatureG": {
    "EnabledFor": [
      {
        "Name": "Microsoft.Percentage",
        "Parameters": {
          "Value": 50
        }
      }
    ]
  }
```

<code>FeatureC</code> makes use of a built-in filter that always enables a feature. <code>FeatureD</code> on the other hand is always disabled as there are no filters enabled in the EnabledFor array. <code>FeatureE</code> makes use of the built in TimeWindow filter that enabled a feature for a specified start and end window. <code>FeatureF</code> is enabled for a custom filter which I shall expand on later that enabled the feature when using Chrome or Edge browsers. <code>FeatureG</code> makes use of another built in Percentage filter that allows you to specify a set percentage of users to enable this feature for.

> The built-in TimeWindow can be configured with just start or end in which case the feature will be enabled from or to a given time respectively.

> Notice how for the built-in TimeWindow the name is TimeWindow and yet the other built-in one is Microsoft.Percentage. The names can make use of the short alias but can also be fully qualified which comes handy in case you have conflicts like having defined your own custom Percentage filter.

> The Microsoft.Percentage implementation is sticky out of the box so you can rest assured that once a user has the feature it will be there even if they refresh the page.

### Consuming the feature flags

For the rest of the examples I have made use of the default template for creating a new MVC app in VS 2019. Let's have a look first at how this can be consumed within controllers and action methods.

```CSharp
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
        // Use IFeatureManager if you want features to reflect
        // updates to IConfiguration
        private readonly IFeatureManager _featureManager;
        // Use IFeatureManager if you want features to remain
        // consistent during lifetime of a request
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;

        public HomeController(IFeatureManager featureManager,
         IFeatureManagerSnapshot featureManagerSnapshot)
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
            return View(new ErrorViewModel
            {
               RequestId = Activity.Current?.Id ??
               HttpContext.TraceIdentifier
            });
        }
    }


}
```

As shown in the example you can annotate a controller or controller action method with the <code>Feature</code> attribute and a feature name, in this case an enum value. Additionally you can grab IFeatureManager and IFeatureManagerSnapshot from the DI and make use of the IsEnabled method to probe feature flag states. Use IFeatureManager if you want features to reflect updates to IConfiguration and IFeatureManager if you want features to remain consistent during lifetime of a request.

### Handling disabled feature flags in controller action methods

By default there an implementation of IDisabledFeaturesHandler wired up that returns 404. If you want to do something custom you can implement IDisabledFeaturesHandler. Below is as sample that redirects to FeatureNotAvailable view.

```CSharp
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;

namespace FeatureManagementRecipes.Features
{
    public class DisabledFeaturesHandler: IDisabledFeaturesHandler
    {
        public Task HandleDisabledFeatures(IEnumerable<string> disabledFeatures,
        ActionExecutingContext context)
        {
            context.Result = new RedirectResult("FeatureNotAvailable");
            return Task.CompletedTask;
        }
    }
}

    // Add this in Startup ConfigureServices
    services.AddFeatureManagement()
            .UseDisabledFeaturesHandler(new DisabledFeaturesHandler());
```

### Custom feature filter

Filters can be creating by implementing IFeatureFilter and registering in the Startup and indicating what feature flags it affects appsettings.json as we did with FeatureF earlier.

```CSharp
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

            return settings == null ||
            context.FeatureName != nameof(ApplicationFeatureFlags.FeatureA) ||
             settings.AllowedBrowsers.Contains(_hardCodedBrowserName);
        }
    }
}

    // Add this in Startup ConfigureServices
    services.AddFeatureManagement()
            .AddFeatureFilter<BrowserFilter>();
```

### Using feature flags in Razor

The library comes with a tag helper that allows you to use features in Razor views.

```html
<feature name="@nameof(ApplicationFeatureFlags.FeatureA)">
  <h1>This is only available if FeatureA is enabled</h1>
</feature>
```

For this to work add the following in \_ViewImports.cshtml

```html
@addTagHelper *, Microsoft.FeatureManagement.AspNetCore
```

### Controlling app flow/bootstrapping

You can further make use of feature flags in your bootstrapping to control app flow. You can control what routes are mapped and what middleware is registered via feature flags. All this with the added beauty that during runtime if your features change in the configuration source so will the routes and middleware.

```CSharp
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;

namespace FeatureManagementRecipes.Features
{
    public class DisabledFeaturesHandler: IDisabledFeaturesHandler
    {
        public Task HandleDisabledFeatures(IEnumerable<string> disabledFeatures,
         ActionExecutingContext context)
        {
            context.Result = new RedirectResult("FeatureNotAvailable");
            return Task.CompletedTask;
        }
    }
}

    // Add this in Startup ConfigureServices
    services.AddFeatureManagement()
            .UseDisabledFeaturesHandler(new DisabledFeaturesHandler());
```

### Conclusion

These are all the features I have had a chance to play with and that may actually be all there is to it. It is looking very promising and the team behind this is currently taking advice from the community to further improve this. There is still a chance there will be naming changes and the API may also change as they figure out the best version of this. If I need feature flags in the near future I would definitely be keen to give what will hopefully be a none preview version of this a test drive. The Azure App Feature Manager as a service also looks very exciting and I am keen to get a chance to try it out also.

The biggest selling point for this library is how well it integrates with asp.net core and how easy it is to extend with filters. I struggle to even think of a case with feature flags that I would not be able to achieve with this. Overall it also is very easy to grasp the concepts and use the library.

Checkout the complete code sample used in this post [on github](https://github.com/chivandikwa/Microsoft.FeatureManagement-and-Microsoft.FeatureManagement.AspNetCore-test-drive).
