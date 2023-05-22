using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class ReflectionHelperTest
    {
        [Theory]
        [InlineData(DateTimeKind.Utc)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Local)]
        public void SetDateTime_Kind(DateTimeKind kind)
        {
            // arrange
            var instance = new TestClass();
            var value = DateTime.Now;
            var member = ReflectionHelper.GetMemberTree<TestClass>(x => x.Created);
            value = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

            // act
            ReflectionHelper.SetMemberValue(member.Last(), value, instance, kind);

            // assert
            Assert.Equal(kind, instance.Created.Kind);
        }

        private class TestClass
        {
            public DateTime Created { get; set; }
        }
    }
}
