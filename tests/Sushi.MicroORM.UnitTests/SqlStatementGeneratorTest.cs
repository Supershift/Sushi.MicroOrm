using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class SqlStatementGeneratorTest
    {
        [Fact]
        public void SelectSingleRowTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);
            query.Add(x => x.Id, 1);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.SingleRow, map, query);
            var sql = statement.ToString();

            // assert
            string expected = @"SELECT TOP(1) ID,Name
FROM MyTable
WHERE ID = @C0";
            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@C0", statement.Parameters[0].Name);
            Assert.Equal(1, statement.Parameters[0].Value);
        }

        [Fact]
        public void SelectMultipleRowsTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);
            query.Add(x => x.Id, 1, ComparisonOperator.GreaterThanOrEquals);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows, map, query);
            var sql = statement.ToString();

            // assert
            string expected = @"SELECT ID,Name
FROM MyTable
WHERE ID >= @C0";
            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@C0", statement.Parameters[0].Name);
            Assert.Equal(1, statement.Parameters[0].Value);
        }

        [Fact]
        public void SelectMultipleRowsTest_MaxResults()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);
            query.MaxResults = 10;
            query.Add(x => x.Id, 1, ComparisonOperator.GreaterThanOrEquals);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows, map, query);
            var sql = statement.ToString();

            // assert
            string expected = @"SELECT TOP(10) ID,Name
FROM MyTable
WHERE ID >= @C0";
            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@C0", statement.Parameters[0].Name);
            Assert.Equal(1, statement.Parameters[0].Value);
        }

        [Fact]
        public void SelectMultipleRowsTest_Paging()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);            
            query.Add(x => x.Id, 1, ComparisonOperator.GreaterThanOrEquals);
            query.Paging = new PagingData() { NumberOfRows = 10, PageIndex = 2 };


            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows, map, query);
            var sql = statement.ToString();

            // assert
            string expected = @"SELECT ID,Name
FROM MyTable
WHERE ID >= @C0
ORDER BY ID OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
SELECT COUNT(*)
FROM MyTable
WHERE ID >= @C0";
            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@C0", statement.Parameters[0].Name);
            Assert.Equal(1, statement.Parameters[0].Value);
        }

        [Fact]
        public void SelectMultipleRowsTest_InvalidPagingException()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new DataMap<MyClass>();
            map.Table("MyTable");
            var query = new DataQuery<MyClass>(map);
            
            query.Paging = new PagingData() { NumberOfRows = 10, PageIndex = 2 };

            // act
            var act = () =>
            {
                var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows, map, query);
            };
            
            // assert
            Assert.Throws<InvalidQueryException>(act);
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
                Table("MyTable");
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name");
            }
        }

        public class MyMapWithAlias : DataMap<MyClass>
        {
            public MyMapWithAlias()
            {
                Table("MyTable");
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name").Alias("FullName");
            }
        }
    }
}
