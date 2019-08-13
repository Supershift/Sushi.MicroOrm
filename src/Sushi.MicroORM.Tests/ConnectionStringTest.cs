using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Tests
{
    [TestClass]
    public class ConnectionStringTest
    {        
        [TestMethod]
        public void FetchMappedByPartOfName()
        {
            var connector = new Connector<DAL.Customers.Customer>();
            var customer = connector.FetchSingle(1);

            Console.WriteLine($"{customer.ID} - {customer.Name}");

            Assert.IsTrue(customer?.ID > 0);
        }

        [TestMethod]
        public void FetchMappedByType()
        {
            var connector = new Connector<DAL.Customers.Address>();
            var address = connector.FetchSingle(1);

            Console.WriteLine($"{address.ID} - {address.Street} {address.Number}, {address.Street}");

            Assert.IsTrue(address?.ID > 0);

            connector = new Connector<DAL.Customers.Address>();
            address = connector.FetchSingle(1);
        }
    }
}
