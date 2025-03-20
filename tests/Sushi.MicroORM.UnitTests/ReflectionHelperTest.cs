﻿using Castle.Core.Logging;
using Moq;
using Sushi.MicroORM.Converters;
using Sushi.MicroORM.Supporting;

namespace Sushi.MicroORM.UnitTests
{
    public class ReflectionHelperTest
    {
        [Fact]
        public void SetDoubleFromDecimal()
        {
            // arrange
            var instance = new TestClass();
            decimal value = 12.46M;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.DoubleValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

            // assert            
            Assert.Equal(decimal.ToDouble(value), instance.DoubleValue);
        }

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
            ReflectionHelper.SetMemberValue(member.Last(), value, instance, kind, null);

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
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

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
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

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
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubProperty!.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

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
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubProperty!.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

            // assert
            Assert.Null(instance.NullableSubProperty);            
        }

        [Fact]
        public void SetNullableSubField_Value()
        {
            // arrange
            var instance = new TestClass();
            int? value = 12;
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubField!.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

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
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.NullableSubField!.SomeValue);

            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, null);

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
            ReflectionHelper.SetMemberValue(memberTree, 29, instance, null, null);

            // assert
            Assert.Equal(29, instance.MutableValue);
        }

        [Fact]
        public void SetUsingConverter()
        {
            // arrange
            var instance = new TestClass();
            string value = "Test";
            string expected = "Converted";
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.Name);
            var converter = new Mock<IConverter>();
            converter.Setup(x=>x.FromDb(value, typeof(string))).Returns(expected);
            
            // act
            ReflectionHelper.SetMemberValue(memberTree, value, instance, null, converter.Object);

            // assert            
            Assert.Equal(expected, instance.Name);
        }

        [Fact]
        public void GetMemberValue()
        {
            // arrange
            var instance = new TestClass() { Name = "Joe"};
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.Name);

            // act
            var result = ReflectionHelper.GetMemberValue(memberTree, instance, null);

            // assert
            Assert.Equal(instance.Name, result);
        }

        [Fact]
        public void GetMemberValue_Nested()
        {
            // arrange
            var instance = new TestClass();
            instance.SubProperty = new SubTestClass(12);
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.SubProperty.SomeValue);

            // act
            var result = ReflectionHelper.GetMemberValue(memberTree, instance, null);

            // assert
            Assert.Equal(instance.SubProperty.SomeValue, result);
        }

        [Fact]
        public void GetMemberValue_WithConverter()
        {
            // arrange
            var instance = new TestClass();
            instance.SubProperty = new SubTestClass(12);            
            var memberTree = ReflectionHelper.GetMemberTree<TestClass>(x => x.SubProperty.SomeValue);

            string expected = "Converted";

            var converter = new Mock<IConverter>();            
            converter.Setup(x=>x.ToDb(instance.SubProperty.SomeValue, typeof(int))).Returns(expected);

            // act
            var result = ReflectionHelper.GetMemberValue(memberTree, instance, converter.Object);

            // assert
            Assert.Equal(expected, result);
        }

        private class TestClass
        {
            public DateTime Created { get; set; }
            public SubTestClass SubProperty { get; set; } = null!;
            public SubTestClass? NullableSubProperty { get; set; }
            public SubTestClass? NullableSubField = null;
            public double? DoubleValue { get; set; }
            public string? Name { get; set; } 
        }

        private class SubTestClass
        {
            private SubTestClass() { }

            public SubTestClass(int someValue)
            {
                SomeValue = someValue;
            }

            public int SomeValue { get; private set; }
        }        
    }
}
