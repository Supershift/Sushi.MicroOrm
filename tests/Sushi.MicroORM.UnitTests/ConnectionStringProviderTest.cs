using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.UnitTests
{   
    public class ConnectionStringProviderTest
    {
        [Fact]
        public void GetCachedConnectionString()
        {
            var provider = new ConnectionStringProvider("d");

            provider.AddMappedConnectionString(typeof(object).ToString(), "a");
            var resultA1 = provider.GetConnectionString(typeof(object));
            var resultA2 = provider.GetConnectionString(typeof(object));

            Assert.Equal(resultA1, resultA2);
        }

        [Fact]
        public void ReplaceConnectionString()
        {
            var provider = new ConnectionStringProvider("d");
            
            provider.AddMappedConnectionString(typeof(object).ToString(), "a");
            var resultA = provider.GetConnectionString(typeof(object));

            provider.AddMappedConnectionString(typeof(object).ToString(), "b");
            var resultB = provider.GetConnectionString(typeof(object));

            Assert.NotEqual(resultA, resultB);
            Assert.Equal("b", resultB);
        }
    }
}
