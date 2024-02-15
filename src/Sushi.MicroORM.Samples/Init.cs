using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sushi.MicroORM.Samples
{
    [TestClass]
    public class Init
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            string settingsFile = System.IO.Directory.GetCurrentDirectory() + "\\appsettings.json";

            if (System.IO.File.Exists(settingsFile))
            {
                //configure using appsettings.json (local testing)
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile(settingsFile);

                var configuration = configurationBuilder.Build();

                string connectionString = configuration.GetConnectionString("TestDatabase");
                DatabaseConfiguration.SetDefaultConnectionString(connectionString);
            }
            else
            {
                //configure using environment variables (build pipeline)
                string connectionString = Environment.GetEnvironmentVariable("TestDatabase");
                DatabaseConfiguration.SetDefaultConnectionString(connectionString);
            }
        }
    }
}