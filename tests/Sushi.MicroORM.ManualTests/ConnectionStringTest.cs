using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests
{
    [TestClass]
    public class ConnectionStringTest
    {   
        [TestMethod]
        public void GetCachedConnectionString()
        {
            var provider = new ConnectionStringProvider();

            provider.AddMappedConnectionString(typeof(object).ToString(), "a");
            var resultA1 = provider.GetConnectionString(typeof(object));
            var resultA2 = provider.GetConnectionString(typeof(object));

            Assert.AreSame(resultA1, resultA2);
        }

        [TestMethod]
        public void ReplaceConnectionString()
        {
            var provider = new ConnectionStringProvider();
            
            provider.AddMappedConnectionString(typeof(object).ToString(), "a");
            var resultA = provider.GetConnectionString(typeof(object));

            provider.AddMappedConnectionString(typeof(object).ToString(), "b");
            var resultB = provider.GetConnectionString(typeof(object));

            Assert.AreNotEqual(resultA, resultB);
            Assert.AreEqual("b", resultB);
        }
    }
}
