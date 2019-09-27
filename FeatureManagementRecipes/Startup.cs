using FeatureManagementRecipes.Features;
using FeatureManagementRecipes.Middleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FeatureManagementRecipes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc(options => options.Filters.AddForFeature<RequireHttpsAttribute>(nameof(ApplicationFeatureFlags.FeatureA)))
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddFeatureManagement()
                    .AddFeatureFilter<BrowserFilter>()
                    .UseDisabledFeaturesHandler(new DisabledFeaturesRedirectHandler("FeatureNotAvailable"))
                    .AddFeatureFilter<PercentageFilter>() // Built-in
                    .AddFeatureFilter<TimeWindowFilter>();// Built-in

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                // Routes can dynamically be added or removed based on the toggling of feature states
                routes.MapRouteForFeature(nameof(ApplicationFeatureFlags.FeatureB), "betaDefault",
                                          "{controller=Beta}/{action=Index}/{id?}", null, null, null);

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // If the feature is enabled/disabled during runtime, the middleware pipeline can be changed dynamically
            app.UseMiddlewareForFeature<CustomDebugLoggerMiddleware>(nameof(ApplicationFeatureFlags.FeatureA));

            // This is the same as above but using the application branching feature
            app.UseForFeature(nameof(ApplicationFeatureFlags.FeatureA), builder =>
            {
                builder.UseCustomDebugLogger();
            });
        }
    }
}
