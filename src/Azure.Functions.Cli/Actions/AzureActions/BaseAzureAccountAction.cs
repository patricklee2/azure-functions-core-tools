﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Colors.Net;
using Azure.Functions.Cli.Arm;
using Azure.Functions.Cli.Interfaces;
using static Azure.Functions.Cli.Common.OutputTheme;

namespace Azure.Functions.Cli.Actions.AzureActions
{
    abstract class BaseAzureAccountAction : BaseAzureAction
    {
        public readonly ISettings Settings;

        public BaseAzureAccountAction(
            IArmManager armManager,
            ISettings settings)
            : base(armManager)
        {
            Settings = settings;
        }

        public async Task PrintAccountsAsync()
        {
            var tenants = await _armManager.GetTenants();
            var currentSub = Settings.CurrentSubscription;
            var subscriptions = tenants
                .Select(t => t.Subscriptions)
                .SelectMany(s => s)
                .Select(s => new
                {
                    displayName = s.DisplayName,
                    subscriptionId = s.SubscriptionId,
                    isCurrent = s.SubscriptionId.Equals(currentSub, StringComparison.OrdinalIgnoreCase)
                })
                .Distinct();

            if (subscriptions.Any())
            {
                if (!subscriptions.Any(s => s.isCurrent))
                {
                    Settings.CurrentSubscription = subscriptions.First().subscriptionId;
                    currentSub = Settings.CurrentSubscription;
                }

                var currentTenant = tenants.FirstOrDefault(t => t.Subscriptions.Any(s => s.SubscriptionId.Equals(currentSub, StringComparison.OrdinalIgnoreCase)));
                Settings.CurrentTenant = currentTenant?.TenantId;

                var longestName = subscriptions.Max(s => s.displayName.Length) + subscriptions.First().subscriptionId.Length + "( ) ".Length;

                ColoredConsole.WriteLine(string.Format($"{{0, {-longestName}}}   {{1}}", TitleColor("Subscription"), TitleColor("Current")));
                ColoredConsole.WriteLine(string.Format($"{{0, {-longestName}}} {{1}}", "------------", "-------"));

                foreach (var subscription in subscriptions)
                {
                    var current = subscription.subscriptionId.Equals(currentSub, StringComparison.OrdinalIgnoreCase)
                        ? TitleColor(true.ToString()).ToString()
                        : false.ToString();
                    ColoredConsole.WriteLine(string.Format($"{{0, {-longestName}}} {{1}}", $"{subscription.displayName} ({subscription.subscriptionId}) ", current));
                }
            }
        }
    }
}
