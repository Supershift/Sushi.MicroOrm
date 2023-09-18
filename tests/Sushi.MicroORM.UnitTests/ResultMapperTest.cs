using Microsoft.Extensions.Options;
using Moq;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System.Data;
using System.Data.Common;

namespace Sushi.MicroORM.UnitTests
{
    public class ResultMapperTest
    {
        [Fact]
        public void SetResultValuesToObjectTest()
        {
            // arrange
            var map = new TestClass.TestClassMap();
            var instance = new TestClass();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datarecord
            var mockedRecord = new Mock<IDataRecord>();
            mockedRecord.SetupGet(x=>x.FieldCount).Returns(2);
            mockedRecord.Setup(x => x.GetName(0)).Returns("id");
            mockedRecord.Setup(x => x.GetName(1)).Returns("name");
            mockedRecord.Setup(x => x.GetValue(0)).Returns(17);
            mockedRecord.Setup(x => x.GetValue(1)).Returns("John Doe");

            // act
            resultMapper.SetResultValuesToObject(mockedRecord.Object, map, instance);

            // assert
            Assert.Equal(17, instance.Id);
            Assert.Equal("John Doe", instance.Name);
        }

        [Fact]
        public void SetResultValuesToObjectTest_DbNullValue()
        {
            // arrange
            var map = new TestClass.TestClassMap();
            var instance = new TestClass();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datarecord
            var mockedRecord = new Mock<IDataRecord>();
            mockedRecord.SetupGet(x => x.FieldCount).Returns(2);
            mockedRecord.Setup(x => x.GetName(0)).Returns("id");
            mockedRecord.Setup(x => x.GetName(1)).Returns("name");
            mockedRecord.Setup(x => x.GetValue(0)).Returns(17);
            mockedRecord.Setup(x => x.GetValue(1)).Returns(DBNull.Value);

            // act
            resultMapper.SetResultValuesToObject(mockedRecord.Object, map, instance);

            // assert
            Assert.Equal(17, instance.Id);
            Assert.Null(instance.Name);
        }


        [Fact]
        public void SetResultValuesToObjectTest_Alias()
        {
            // arrange
            var map = new MyMapWithAlias();
            var instance = new TestClass();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datarecord
            var mockedRecord = new Mock<IDataRecord>();
            mockedRecord.SetupGet(x => x.FieldCount).Returns(2);
            mockedRecord.Setup(x => x.GetName(0)).Returns("id");
            mockedRecord.Setup(x => x.GetName(1)).Returns("fullName");
            mockedRecord.Setup(x => x.GetValue(0)).Returns(17);
            mockedRecord.Setup(x => x.GetValue(1)).Returns("John Doe");

            // act
            resultMapper.SetResultValuesToObject(mockedRecord.Object, map, instance);

            // assert
            Assert.Equal(17, instance.Id);
            Assert.Equal("John Doe", instance.Name);
        }

        [Fact]
        public async Task MapToSingleResultScalarTest()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockedReader.Setup(x => x.GetValue(0)).Returns(17);

            // act
            var result = await resultMapper.MapToSingleResultScalarAsync<int>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(17, result);
        }

        [Fact]
        public async Task MapToSingleResultScalarTest_NullableEnum()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockedReader.Setup(x => x.GetValue(0)).Returns((int)TestEnum.Green);

            // act
            var result = await resultMapper.MapToSingleResultScalarAsync<TestEnum?>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(TestEnum.Green, result);
        }

        [Fact]
        public async Task MapToSingleResultScalarTest_NotFound()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // act
            var result = await resultMapper.MapToSingleResultScalarAsync<int>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(default, result);
        }

        [Fact]
        public async Task MapToSingleResultScalarTest_TypeMismatch()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockedReader.Setup(x => x.GetValue(0)).Returns("i am a string");

            // act
            var result = await resultMapper.MapToSingleResultScalarAsync<int>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(default, result);
        }

        [Fact]
        public async Task MapToMultipleResultsScalarTest()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockedReader.SetupSequence(x => x.GetValue(0))
                .Returns(17)
                .Returns(11)
                .Throws(new IndexOutOfRangeException());

            // act
            var result = await resultMapper.MapToMultipleResultsScalarAsync<int>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(17, result[0]);
            Assert.Equal(11, result[1]);
        }

        [Fact]
        public async Task MapToMultipleResultsScalarTest_TypeMismatch()
        {
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();
            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockedReader.SetupSequence(x => x.GetValue(0))
                .Returns(17)
                .Returns("i am a string")
                .Throws(new IndexOutOfRangeException());

            // act
            var result = await resultMapper.MapToMultipleResultsScalarAsync<int>(mockedReader.Object, CancellationToken.None);

            // assert
            Assert.Equal(17, result[0]);
            Assert.Equal(default, result[1]);
        }


        [Fact]
        public async Task MapToSingleResultTest()
        {
            var map = new TestClass.TestClassMap();            
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            var result = await resultMapper.MapToSingleResultAsync(mockedReader.Object, map, CancellationToken.None);

            // assert
            Assert.NotNull(result);
            Assert.IsType<TestClass>(result);
        }

        [Fact]
        public async Task MapToSingleResultTest_DefaultNull()
        {
            var map = new TestClass.TestClassMap();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))                
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            var result = await resultMapper.MapToSingleResultAsync(mockedReader.Object, map, CancellationToken.None);

            // assert
            Assert.Null(result);            
        }

        [Fact]
        public async Task MapToSingleResultTest_PrivateConstructor()
        {
            var map = new TestClassPrivateConstructor.TestClassMap();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            var result = await resultMapper.MapToSingleResultAsync(mockedReader.Object, map, CancellationToken.None);

            // assert
            Assert.NotNull(result);
            Assert.IsType<TestClassPrivateConstructor>(result);
        }

        [Fact]
        public async Task MapToSingleResultTest_NonParameterlessConstructor_Exception()
        {
            var expectedExceptionMsg = $"Please use parameterless constructor. (Parameter '{nameof(TestClassNonParameterlessConstructor)}')";
            var map = new TestClassNonParameterlessConstructor.TestClassMap();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            Func<Task> act = async () => await resultMapper.MapToSingleResultAsync(mockedReader.Object, map, CancellationToken.None);

            // assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(act);
            Assert.Equal(expectedExceptionMsg, exception.Message);
        }

        [Fact]
        public async Task MapToSingleResultTest_DefaultInstance()
        {
            var map = new DataMap<MyStruct>();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            var result = await resultMapper.MapToSingleResultAsync(mockedReader.Object, map, CancellationToken.None);

            // assert            
            Assert.IsType<MyStruct>(result);
        }

        [Fact]
        public async Task MapToMultipleResultsTest()
        {
            var map = new TestClass.TestClassMap();
            var resultMapper = new ResultMapper(DefaultOptions);

            // create mocked datareader
            var mockedReader = new Mock<DbDataReader>();

            mockedReader.SetupSequence(x => x.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockedReader.SetupGet(x => x.FieldCount).Returns(0);

            // act
            var result = await resultMapper.MapToMultipleResultsAsync(mockedReader.Object, map, CancellationToken.None);

            // assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, Assert.NotNull);
            Assert.NotEqual(result[0], result[1]);
        }

        // test fakes
        public class MyMapWithAlias : DataMap<TestClass>
        {
            public MyMapWithAlias()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name").Alias("FullName");
            }
        }

        public IOptions<MicroOrmOptions> DefaultOptions = Options.Create(new MicroOrmOptions());

        public struct MyStruct { };
    }
}