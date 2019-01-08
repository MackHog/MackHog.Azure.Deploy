using Microsoft.Azure.Management.AppService.Fluent;
using System;

namespace Fluent.Deploy
{
    public class Config
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
        public string RegionName { get; set; }
        public string ResourceGroupName { get; set; }
        public string WebAppName { get; set; }
        public string AppServicePlanName { get; set; }
        public string AppServicePlanPricingTierName { get; set; }
        public PricingTier AppServicePricingTier { get; set; }
        public Microsoft.Azure.Management.AppService.Fluent.OperatingSystem AppServiceOperationSystem => Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Windows;
        public Config() { }

        public void Validate()
        {
            ValidateRegionName();
            AppServicePricingTier = GetAppServicePlanPricingTier();
        }

        private PricingTier GetAppServicePlanPricingTier()
        {
            switch (AppServicePlanPricingTierName)
            {
                case "Free_F1":
                    return PricingTier.FreeF1;
                case "BasicB1":
                    return PricingTier.BasicB1;
                case "BasicB2":
                    return PricingTier.BasicB2;
                case "BasicB3":
                    return PricingTier.BasicB3;
                case "PremiumP1":
                    return PricingTier.PremiumP1;
                case "PremiumP2":
                    return PricingTier.PremiumP2;
                case "PremiumP3":
                    return PricingTier.PremiumP3;
                case "SharedD1":
                    return PricingTier.SharedD1;
                case "StandardS1":
                    return PricingTier.StandardS1;
                case "StandardS2":
                    return PricingTier.StandardS2;
                case "StandardS3":
                    return PricingTier.StandardS3;
                default:
                    return PricingTier.FreeF1;
            }
        }

        private void ValidateRegionName()
        {
            string allNames = "centralus,eastasia,southeastasia,eastus,eastus2,westus,westus2,northcentralus,southcentralus,westcentralus,northeurope,westeurope,japaneast,japanwest,brazilsouth,australiasoutheast,australiaeast,westindia,southindia,centralindia,canadacentral,canadaeast,uksouth,ukwest,koreacentral,koreasouth,francecentral";
            if (!allNames.Contains(RegionName.ToLower()))
                throw new FormatException($"RegionName {RegionName} is not valid. Valid regions {allNames}");
        }
    }
}
