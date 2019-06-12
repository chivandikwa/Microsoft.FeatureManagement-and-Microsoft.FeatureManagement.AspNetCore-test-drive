using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;

namespace FeatureManagementRecipes.Features
{
    internal class DisabledFeaturesRedirectHandler: IDisabledFeaturesHandler
    {
        private readonly string _redirectUri;

        public DisabledFeaturesRedirectHandler(string redirectUri)
        {
            _redirectUri = redirectUri;
        }

        public Task HandleDisabledFeatures(IEnumerable<string> disabledFeatures, ActionExecutingContext context)
        {
            context.Result = new RedirectResult(_redirectUri);
            return Task.CompletedTask;
        }
    }
}