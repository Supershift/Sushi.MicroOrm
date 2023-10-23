using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class SqlStatementTest
    {
        [Fact]
        public void ToStringTest_Select()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows);
            statement.DmlClause = "SELECT *";
            statement.FromClause = "FROM MyTable";
            statement.WhereClause = "WHERE ID > 10";            

            // act
            var query = statement.ToString();

            // assert
            string expected = @"SELECT *
FROM MyTable
WHERE ID > 10";

            Assert.Equal(expected, query);
        }

        [Fact]
        public void ToStringTest_SelectOrderBy()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows);
            statement.DmlClause = "SELECT *";
            statement.FromClause = "FROM MyTable";
            statement.WhereClause = "WHERE ID > 10";
            statement.OrderByClause = "ORDER BY ID";

            // act
            var query = statement.ToString();

            // assert
            string expected = @"SELECT *
FROM MyTable
WHERE ID > 10
ORDER BY ID";

            Assert.Equal(expected, query);
        }

        [Fact]
        public void ToStringTest_SelectWithPaging()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.Select, SqlStatementResultCardinality.MultipleRows);
            statement.DmlClause = "SELECT *";
            statement.FromClause = "FROM MyTable";
            statement.WhereClause = "WHERE ID > 10";
            statement.OrderByClause = "ORDER BY ID";
            statement.AddPagingRowCountStatement = true;

            // act
            var query = statement.ToString();

            // assert
            string expected = @"SELECT *
FROM MyTable
WHERE ID > 10
ORDER BY ID
SELECT COUNT(*)
FROM MyTable
WHERE ID > 10";

            Assert.Equal(expected, query);
        }

        [Fact]
        public void ToStringTest_Insert()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.Insert, SqlStatementResultCardinality.None);
            statement.DmlClause = "INSERT";
            statement.InsertIntoClause = "INTO MyTable(ID,Name)";
            statement.OutputClause = $"OUTPUT inserted.ID";
            statement.InsertValuesClause = "DEFAULT VALUES";
            
            statement.AddPagingRowCountStatement = true;

            // act
            var query = statement.ToString();

            // assert
            string expected = @"INSERT
INTO MyTable(ID,Name)
OUTPUT inserted.ID
DEFAULT VALUES";

            Assert.Equal(expected, query);
        }

        [Fact]
        public void ToStringTest_Update()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.Update, SqlStatementResultCardinality.None);
            statement.DmlClause = "UPDATE MyTable";
            statement.UpdateSetClause = "SET Name = 'Test'";
            statement.OutputClause = $"OUTPUT inserted.ID";
            statement.FromClause = "FROM MyTable";
            statement.WhereClause = "WHERE ID = 10";            
            statement.AddPagingRowCountStatement = true;

            // act
            var query = statement.ToString();

            // assert
            string expected = @"UPDATE MyTable
SET Name = 'Test'
OUTPUT inserted.ID
FROM MyTable
WHERE ID = 10";

            Assert.Equal(expected, query);
        }

        [Fact]
        public void ToStringTest_CustomQuery()
        {
            // arrange
            var statement = new SqlStatement(DMLStatementType.CustomQuery, SqlStatementResultCardinality.SingleRow);
            statement.CustomSqlStatement = @"UPDATE MyTable
SET Name = 'Test'
FROM MyTable
WHERE ID = 10";            

            // act
            var query = statement.ToString();

            // assert
            Assert.Equal(statement.CustomSqlStatement, query);
        }
    }
}
