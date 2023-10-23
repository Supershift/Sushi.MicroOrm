using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class ExceptionHandlerTest
    {
        [Fact]
        public void HandleExceptionTest()
        {
            // arrange
            var handler = new ExceptionHandler();
            var exception = new Exception("Some error");
            // act
            var result = handler.Handle(exception, null);
            // assert
            Assert.NotEqual(exception, result);
            Assert.Equal(exception, result.InnerException);
            Assert.Equal("Some error", result.Message);
        }

        [Fact]
        public void HandleExceptionTest_WithStatement()
        {
            // arrange
            var handler = new ExceptionHandler();
            var exception = new Exception("Some error");
            var statement = new SqlStatement(DMLStatementType.Select, SqlStatementResultCardinality.SingleRow);
            
            // act
            var result = handler.Handle(exception, statement);
            
            // assert
            Assert.NotEqual(exception, result);
            Assert.Equal(exception, result.InnerException);
            Assert.NotEqual(exception.Message, result.Message);
        }
    }
}
