using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class SqlServerConnectionStringTest
    {
        [Fact]
        public void CreateConnectionString_UseReadOnly()
        {
            // arrange
            string primary = "Server=.;Database=master;Trusted_Connection=True;";
            string expectedReadOnly = "Server=.;Database=master;Trusted_Connection=True;ApplicationIntent=ReadOnly;";

            // act
            var result = new SqlServerConnectionString(primary, true);

            // assert
            Assert.Equal(primary, result.Primary);
            Assert.Equal(expectedReadOnly, result.ReadOnly);
        }

        [Fact]
        public void CreateConnectionString_UseReadOnly2()
        {
            // arrange
            string primary = "Server=.;Database=master;Trusted_Connection=True";
            string expectedReadOnly = "Server=.;Database=master;Trusted_Connection=True;ApplicationIntent=ReadOnly;";

            // act
            var result = new SqlServerConnectionString(primary, true);

            // assert
            Assert.Equal(primary, result.Primary);
            Assert.Equal(expectedReadOnly, result.ReadOnly);
        }
    }
}
