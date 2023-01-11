using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class UtilityTest
    {
        [Theory]
        [InlineData(typeof(int), SqlDbType.Int)]
        [InlineData(typeof(long), SqlDbType.BigInt)]
        [InlineData(typeof(short), SqlDbType.SmallInt)]
        [InlineData(typeof(byte), SqlDbType.TinyInt)]
        [InlineData(typeof(bool), SqlDbType.Bit)]
        [InlineData(typeof(Guid), SqlDbType.UniqueIdentifier)]
        [InlineData(typeof(DateTime), SqlDbType.DateTime)]
        [InlineData(typeof(DateTimeOffset), SqlDbType.DateTimeOffset)]
        [InlineData(typeof(DateOnly), SqlDbType.Date)]
        [InlineData(typeof(decimal), SqlDbType.Decimal)]
        [InlineData(typeof(double), SqlDbType.Decimal)]
        [InlineData(typeof(float), SqlDbType.Decimal)]
        [InlineData(typeof(TimeSpan), SqlDbType.Time)]
        [InlineData(typeof(TimeOnly), SqlDbType.Time)]
        [InlineData(typeof(Byte[]), SqlDbType.VarBinary)]
        public void GetSqlDbTypeTest(Type type, SqlDbType expected)
        {
            var result = Utility.GetSqlDbType(type);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetSqlDbTypeTest_Unbox()
        {
            var type = typeof(int?);

            var result = Utility.GetSqlDbType(type);

            Assert.Equal(SqlDbType.Int, result);
        }

        [Fact]
        public void GetSqlDbTypeTest_Enum()
        {
            var type = typeof(MyEnum);

            var result = Utility.GetSqlDbType(type);

            Assert.Equal(SqlDbType.Int, result);
        }

        [Fact]
        public void ConvertValueToEnumTest()
        {
            var value = 1;

            var result = Utility.ConvertValueToEnum(value, typeof(MyEnum));

            Assert.IsType<MyEnum>(result);
            Assert.Equal(MyEnum.OptionA, result);
        }

        [Fact]
        public void ConvertValueToEnumTest_NotAnEnum()
        {
            var value = 1;

            var result = Utility.ConvertValueToEnum(value, typeof(int));

            Assert.IsType<int>(result);
            Assert.Equal(value, result);
        }

        private enum MyEnum
        {
            OptionA = 1,
            OptionB = 2
        }
    }
}
