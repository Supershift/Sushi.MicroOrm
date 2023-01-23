using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void RegisterConnectorDependenciesTest()
        {
            string connectionString = "Server=.;Initial Catalog=db;User ID=user;Password=pass;";
            
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroORM(connectionString);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // create default connector
            var connector = serviceProvider.GetService<IConnector<object>>();

            Assert.NotNull(connector);
        }

        [Fact]
        public void DefaultConnectionStringTest()
        {
            string connectionString = "Server=.;Initial Catalog=db;User ID=user;Password=pass;";

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMicroORM(connectionString);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // get connection string provider
            var connectionStringProvider = serviceProvider.GetRequiredService<ConnectionStringProvider>();

            // check default connection string is set
            Assert.Equal(connectionString, connectionStringProvider.DefaultConnectionString);
        }

        [Fact]
        public void ConfigurationBuilderTest_IsCalled()
        {
            string connectionString = "Server=.;Initial Catalog=db;User ID=user;Password=pass;";

            IServiceCollection serviceCollection = new ServiceCollection();

            bool isConfigBuilderCalled = false;
            serviceCollection.AddMicroORM(connectionString, c=>
            {
                isConfigBuilderCalled = true;
            }); 
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // get connection string provider
            var connectionStringProvider = serviceProvider.GetRequiredService<ConnectionStringProvider>();

            // check default connection string is set
            Assert.True(isConfigBuilderCalled);
        }

        [Fact]
        public void ConfigurationBuilderTest_IsSameConnectionStringProvider()
        {
            string connectionString = "Server=.;Initial Catalog=db;User ID=user;Password=pass;";

            IServiceCollection serviceCollection = new ServiceCollection();

            ConnectionStringProvider? configConnectionStringProvider = null;
            serviceCollection.AddMicroORM(connectionString, c =>
            {
                configConnectionStringProvider = c.ConnectionStringProvider;
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // get connection string provider
            var connectionStringProvider = serviceProvider.GetRequiredService<ConnectionStringProvider>();

            // check default connection string is set
            Assert.NotNull(configConnectionStringProvider);
            Assert.Equal(configConnectionStringProvider, connectionStringProvider);
        }
    }
}
