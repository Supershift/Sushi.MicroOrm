using Sushi.MicroORM.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class JsonConverterTest
    {
        [Fact]
        public void FromDb_StringArray()
        {
            // arrange
            string dbValue = "[\"one\",\"two\",\"three\"]";
            var expected = new string[] { "one", "two", "three" };
            var expectedType = expected.GetType();
            var converter = new JsonConverter();

            // act
            var result = converter.FromDb(dbValue, expectedType);

            // assert
            Assert.IsType(expectedType, result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FromDb_EmptyString()
        {
            // arrange
            string dbValue = "";
            object? expected = null;      
            var type = typeof(string[]);
            var converter = new JsonConverter();

            // act
            var result = converter.FromDb(dbValue, type);

            // assert            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FromDb_Null()
        {
            // arrange
            string? dbValue = null;
            object? expected = null;
            var type = typeof(string[]);
            var converter = new JsonConverter();

            // act
            var result = converter.FromDb(dbValue, type);

            // assert            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FromStringArray()
        {
            // arrange
            var value = new string[] { "one", "two", "three" };
            var expected = "[\"one\",\"two\",\"three\"]";

            var converter = new JsonConverter();

            // act
            var result = converter.ToDb(value, value.GetType());

            // assert            
            Assert.Equal(expected, result);
        }
    }
}
