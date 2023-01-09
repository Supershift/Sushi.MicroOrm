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
            var map = new MyMap();
            var instance = new MyClass();
            var resultMapper = new ResultMapper();

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
        public void SetResultValuesToObjectTest_Alias()
        {
            // arrange
            var map = new MyMapWithAlias();
            var instance = new MyClass();
            var resultMapper = new ResultMapper();

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
            var resultMapper = new ResultMapper();

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
        public async Task MapToSingleResultScalarTest_NotFound()
        {
            var resultMapper = new ResultMapper();

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
            var resultMapper = new ResultMapper();

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
            var resultMapper = new ResultMapper();

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
            var resultMapper = new ResultMapper();

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
            var map = new MyMap();            
            var resultMapper = new ResultMapper();

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
            Assert.IsType<MyClass>(result);
        }

        [Fact]
        public async Task MapToSingleResultTest_DefaultNull()
        {
            var map = new MyMap();
            var resultMapper = new ResultMapper();

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
        public async Task MapToSingleResultTest_DefaultInstance()
        {
            var map = new DataMap<MyStruct>();
            var resultMapper = new ResultMapper();

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
            var map = new MyMap();
            var resultMapper = new ResultMapper();

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

        public class MyClass
        {
            public int Id { get; set; }
            public string? Name { get; set; }    
        }

        public class MyMap : DataMap<MyClass>
        {
            public MyMap()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name");
            }
        }

        public class MyMapWithAlias : DataMap<MyClass>
        {
            public MyMapWithAlias()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name").Alias("FullName");
            }
        }

        public struct MyStruct { };
    }
}