using Castle.Core.Logging;
using Sushi.MicroORM.Supporting;

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

        [Fact]
        public void SetSubProperty_Value()
        {
            // arrange
            var instance = new TestClass();
            var value = 12;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.SubProperty.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.NotNull(instance.SubProperty);
            Assert.Equal(value, instance.SubProperty.SomeValue);
        }

        [Fact]
        public void SetSubProperty_NullValue()
        {
            // arrange
            var instance = new TestClass();
            int? value = null;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.SubProperty.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.NotNull(instance.SubProperty);
            Assert.Equal(0, instance.SubProperty.SomeValue);
        }

        [Fact]
        public void SetNullableSubProperty_Value()
        {
            // arrange
            var instance = new TestClass();
            int? value = 12;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubProperty.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.NotNull(instance.NullableSubProperty);
            Assert.Equal(value, instance.NullableSubProperty.SomeValue);
        }

        [Fact]
        public void SetNullableSubProperty_NullValue()
        {
            // arrange
            var instance = new TestClass();
            int? value = null;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubProperty.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.Null(instance.NullableSubProperty);            
        }

        [Fact]
        public void SetNullableSubField_Value()
        {
            // arrange
            var instance = new TestClass();
            int? value = 12;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubField.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.NotNull(instance.NullableSubField);
            Assert.Equal(value, instance.NullableSubField.SomeValue);
        }

        [Fact]
        public void SetNullableSubField_NullValue()
        {
            // arrange
            var instance = new TestClass();
            int? value = null;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubField.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null);

            // assert
            Assert.Null(instance.NullableSubField);
        }

        [Fact]
        public void SetMutabableRecordProperty()
        {
            // arrange
            var instance = new TestRecord() { MutableValue = 10};
            var memberTree = ReflectionHelper.GetMemberTree<TestRecord>(x => x.MutableValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, 29, instance, null);

            // assert
            Assert.Equal(29, instance.MutableValue);
        }

        private class TestClass
        {
            public DateTime Created { get; set; }
            public SubTestClass SubProperty { get; set; }
            public SubTestClass? NullableSubProperty { get; set; }
            public SubTestClass? NullableSubField;
        }

        private class SubTestClass
        {
            public int SomeValue { get; set; }
        }        
    }
}
