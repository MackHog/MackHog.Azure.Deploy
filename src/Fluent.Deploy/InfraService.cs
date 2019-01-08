using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluent.Deploy
{
    public class InfraService
    {
        private ILog _log;
        private Config _configuration;

        public InfraService(
            ILog log,
            Config configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        public IAzure SignIn()
        {
            _log.Info("Connecting to Azure subscription");
            var creds = new AzureCredentialsFactory().FromServicePrincipal(
                _configuration.ClientId,
                _configuration.ClientSecret,
                _configuration.TenantId,
                AzureEnvironment.AzureGlobalCloud);
            var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(creds)
                .WithSubscription(_configuration.SubscriptionId);
            _log.Info("Connected to Azure subscription");
            return azure;
        }

        public Task<IResourceGroup> TryFetchResourceGroupAsync(IAzure azureMgn) =>
            SetupAsync<IResourceGroup>(
                "resource group",
                _configuration.ResourceGroupName,
                existAsync: () => azureMgn.ResourceGroups.ContainAsync(_configuration.ResourceGroupName),
                createAsync: () => azureMgn.ResourceGroups
                        .Define(_configuration.ResourceGroupName)
                        .WithRegion(_configuration.RegionName)
                        .CreateAsync(),
                getAsync: () => azureMgn.ResourceGroups.GetByNameAsync(_configuration.ResourceGroupName));

        public Task<IAppServicePlan> TryFetchAppServicePlanAsync(IAzure azureMgn, IResourceGroup rg) =>
            SetupAsync<IAppServicePlan>(
                "app service plan",
                _configuration.AppServicePlanName,
                existAsync: async () => (await azureMgn.AppServices.AppServicePlans.ListByResourceGroupAsync(_configuration.ResourceGroupName))
                .Any(a => a.Name == _configuration.AppServicePlanName),
                createAsync: () => azureMgn.AppServices.AppServicePlans
                        .Define(_configuration.AppServicePlanName)
                        .WithRegion(_configuration.RegionName)
                        .WithExistingResourceGroup(rg)
                        .WithPricingTier(_configuration.AppServicePricingTier)
                        .WithOperatingSystem(_configuration.AppServiceOperationSystem)
                        .CreateAsync(),
                getAsync: () => azureMgn.AppServices.AppServicePlans.GetByResourceGroupAsync(_configuration.AppServicePlanName, _configuration.AppServicePlanName));

        public Task<IWebApp> TryFetchWebAppAsync(IAzure azureMgn, IResourceGroup rg, IAppServicePlan asp) =>
            SetupAsync<IWebApp>(
                "web app",
                _configuration.WebAppName,
                existAsync: async () => (await azureMgn.WebApps.ListByResourceGroupAsync(_configuration.ResourceGroupName))
               .Any(a => a.Name == _configuration.WebAppName),
                createAsync: () => azureMgn.WebApps
                    .Define(_configuration.WebAppName)
                    .WithExistingWindowsPlan(asp)
                    .WithExistingResourceGroup(rg)
                    .WithWebAppAlwaysOn(true)
                    .CreateAsync(),
                getAsync: () => azureMgn.WebApps.GetByResourceGroupAsync(_configuration.ResourceGroupName, _configuration.WebAppName));

        private async Task<T> SetupAsync<T>(
            string type,
            string name,
            Func<Task<bool>> existAsync,
            Func<Task<T>> createAsync,
            Func<Task<T>> getAsync)
        {
            T infraObj;
            _log.Info($"Checking if {type} {name} exists");
            if (!await existAsync())
            {
                _log.Info($"{type} does not exist. Creating...");
                infraObj = await createAsync();
                _log.Info($"{type} created");
            }
            else
            {
                _log.Info($"{type} already exist. Fetching...");
                infraObj = await getAsync();
            }
            return infraObj;
        }
    }
}
