using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests
{
    [TestClass]
    public class ConnectionStringTest
    {        
        [TestMethod]
        public async Task FetchMappedByPartOfName()
        {
            var connector = new Connector<DAL.Customers.Customer>();
            var customer = await connector.GetSingleAsync(1);

            Console.WriteLine($"{customer.ID} - {customer.Name}");

            Assert.IsTrue(customer?.ID > 0);
        }

        [TestMethod]
        public async Task FetchMappedByType()
        {
            var connector = new Connector<DAL.Customers.Address>();
            var address = await connector.GetSingleAsync(1);

            Console.WriteLine($"{address.ID} - {address.Street} {address.Number}, {address.Street}");

            Assert.IsTrue(address?.ID > 0);

            connector = new Connector<DAL.Customers.Address>();
            address = await connector.GetSingleAsync(1);
        }

        [TestMethod]
        public void GetCachedConnectionString()
        {
            var provider = new ConnectionStringProvider();

            provider.AddConnectionString(typeof(object).ToString(), "a");
            var resultA1 = provider.GetConnectionString(typeof(object));
            var resultA2 = provider.GetConnectionString(typeof(object));

            Assert.AreSame(resultA1, resultA2);
        }

        [TestMethod]
        public void ReplaceConnectionString()
        {
            var provider = new ConnectionStringProvider();
            
            provider.AddConnectionString(typeof(object).ToString(), "a");
            var resultA = provider.GetConnectionString(typeof(object));

            provider.AddConnectionString(typeof(object).ToString(), "b");
            var resultB = provider.GetConnectionString(typeof(object));

            Assert.AreNotEqual(resultA, resultB);
            Assert.AreEqual("b", resultB);
        }
    }
}
