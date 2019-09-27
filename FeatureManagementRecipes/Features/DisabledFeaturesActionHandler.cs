using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement.Mvc;

namespace FeatureManagementRecipes.Features
{
    internal class DisabledFeaturesActionHandler : IDisabledFeaturesHandler
    {
        private readonly Action<IEnumerable<string>, ActionExecutingContext> _handler;

        public DisabledFeaturesActionHandler(
            Action<IEnumerable<string>, ActionExecutingContext> handler)
        {
            Action<IEnumerable<string>, ActionExecutingContext> action = handler;
            _handler = action ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleDisabledFeatures(IEnumerable<string> disabledFeatures, ActionExecutingContext context)
        {
            _handler(disabledFeatures, context);
            return Task.CompletedTask;
        }
    }
}