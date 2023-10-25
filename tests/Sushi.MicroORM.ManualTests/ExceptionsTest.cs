using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.ManualTests.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests
{
    [TestClass]
    public class ExceptionsTest
    {   
        private readonly ServiceProvider _serviceProvider;

        public ExceptionsTest()
        {
            var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables()
            .Build();

            // get connection strings
            string connectionString = configuration.GetConnectionString("TestDatabase");            

            // register dependencies
            IServiceCollection serviceCollection = new ServiceCollection();

            // add micro orm
            serviceCollection.AddMicroORM(connectionString, c =>
            {
                
            });

            // build provider
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [TestMethod]
        public async Task UniqueIndexViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(Guid.NewGuid(), 1234);

            // act
            var act = async () => await connector.InsertAsync(uniqueValue);

            // assert
            await Assert.ThrowsExceptionAsync<UniqueIndexViolationException>(act);
        }

        [TestMethod]
        public async Task PrimaryKeyConstraintViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(new Guid("813d3cfd-b582-4ee1-9d2d-bc7e58e456b4"), 574574);

            // act
            var act = async () => await connector.InsertAsync(uniqueValue);

            // assert            
            await Assert.ThrowsExceptionAsync<UniqueConstraintViolationException>(act);
        }

        [TestMethod]
        public async Task ConstraintViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<Parent>>();
            var query = connector.CreateQuery();
            query.Add(x => x.Id, 1);
            
            // act
            var act = async () => await connector.DeleteAsync(query);

            // assert            
            await Assert.ThrowsExceptionAsync<ConstraintViolationException>(act);
        }

        [TestMethod]
        public async Task BulkInsert_UniqueIndexViolation()
        {
            // arrange
            var connector = _serviceProvider.GetRequiredService<IConnector<UniqueValue>>();
            var uniqueValue = new UniqueValue(Guid.NewGuid(), 1234);

            // act
            var act = async () => await connector.BulkInsertAsync(new [] { uniqueValue });

            // assert
            await Assert.ThrowsExceptionAsync<UniqueIndexViolationException>(act);
        }
    }
}
