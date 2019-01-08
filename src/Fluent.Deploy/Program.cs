using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Fluent.Deploy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var log = new Log();
            log.Info($"Start Fluent.Deploy");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var config = new Config();
            configuration.Bind("Config", config);
            config.Validate();

            await CreateEnvironment(log, config);

            log.Info($"Finished Fluent.Deploy");
        }

        private static async Task CreateEnvironment(Log log, Config config)
        {
            var infraService = new InfraService(log, config);
            var azureSub = infraService.SignIn();
            var rg = await infraService.TryFetchResourceGroupAsync(azureSub);
            var asp = await infraService.TryFetchAppServicePlanAsync(azureSub, rg);
            var wa = await infraService.TryFetchWebAppAsync(azureSub, rg, asp);
        }
    }
}
