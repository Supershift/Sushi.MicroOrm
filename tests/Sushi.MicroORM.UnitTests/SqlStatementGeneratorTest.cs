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
        public void InsertSingleRowTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);

            var entity = new MyClass()
            {
                Id = 1,
                Name = "Insert"
            };

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Insert, SqlStatementResultCardinality.None, map, query, entity, true);
            var sql = statement.ToString();

            // assert
            string expected = @"INSERT
INTO MyTable( ID,Name )
VALUES ( @i0,@i1 )";

            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@i0", statement.Parameters[0].Name);
            Assert.Equal(entity.Id, statement.Parameters[0].Value);
            Assert.Equal("@i1", statement.Parameters[1].Name);
            Assert.Equal(entity.Name, statement.Parameters[1].Value);
        }

        [Fact]
        public void InsertOrUpdateSingleRowTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);

            var entity = new MyClass()
            {
                Id = 1,
                Name = "Insert Or Update"
            };

            query.Add(x => x.Id, entity.Id, ComparisonOperator.Equals);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.InsertOrUpdate, SqlStatementResultCardinality.None, map, query, entity, true);
            var sql = statement.ToString();

            // assert
            string expected = @"
IF EXISTS(SELECT * FROM MyTable WHERE ID = @C0)
BEGIN
UPDATE MyTable
SET Name = @u0
FROM MyTable
WHERE ID = @C0
END
ELSE
BEGIN
INSERT
INTO MyTable( ID,Name )
VALUES ( @i0,@i1 )
END";

            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@u0", statement.Parameters[0].Name);
            Assert.Equal(entity.Name, statement.Parameters[0].Value);
            Assert.Equal("@C0", statement.Parameters[1].Name);
            Assert.Equal(entity.Id, statement.Parameters[1].Value);
            Assert.Equal("@i0", statement.Parameters[2].Name);
            Assert.Equal(entity.Id, statement.Parameters[2].Value);
            Assert.Equal("@i1", statement.Parameters[3].Name);
            Assert.Equal(entity.Name, statement.Parameters[3].Value);
        }

        [Fact]
        public void UpdateSingleRowTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);

            var entity = new MyClass()
            {
                Id = 1,
                Name = "Update"
            };

            query.Add(x => x.Id, entity.Id, ComparisonOperator.Equals);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Update, SqlStatementResultCardinality.None, map, query, entity, true);
            var sql = statement.ToString();

            // assert
            string expected = @"UPDATE MyTable
SET Name = @u0
FROM MyTable
WHERE ID = @C0";

            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@u0", statement.Parameters[0].Name);
            Assert.Equal(entity.Name, statement.Parameters[0].Value);
            Assert.Equal("@C0", statement.Parameters[1].Name);
            Assert.Equal(entity.Id, statement.Parameters[1].Value);
        }

        [Fact]
        public void DeleteSingleRowTest()
        {
            // arrange
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);

            var entity = new MyClass()
            {
                Id = 1
            };

            query.Add(x => x.Id, entity.Id, ComparisonOperator.Equals);

            // act
            var statement = generator.GenerateSqlStatment(DMLStatementType.Delete, SqlStatementResultCardinality.None, map, query);
            var sql = statement.ToString();

            // assert
            string expected = @"DELETE 
FROM MyTable
WHERE ID = @C0";

            Assert.Equal(expected, sql);
            Assert.NotNull(statement.Parameters);
            Assert.NotEmpty(statement.Parameters);
            Assert.Equal("@C0", statement.Parameters[0].Name);
            Assert.Equal(entity.Id, statement.Parameters[0].Value);
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
