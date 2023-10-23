using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// Provides methods to handle exceptions thrown when executing commands against the database.
    /// </summary>
    public class ExceptionHandler
    {
        /// <summary>
        /// Creates an exception based on info found on <paramref name="ex"/>. If possible, the exception will be more specialized than the original exception.
        /// </summary>
        /// <param name="ex">Exception thrown when executing a sql statement.</param>
        /// <param name="sqlStatement">Statement that caused the exception.</param>
        /// <returns></returns>
        public Exception Handle(Exception ex, SqlStatement? sqlStatement)
        {
            // generate an error message, including info about the executed statement if available
            string errorMessage;
            if (sqlStatement != null)
            {
                var parameters = string.Join(Environment.NewLine, sqlStatement.Parameters.Select(x=>$"{x.Name} = '{x.Value}' ({x.Type})"));                
                errorMessage = $"Error while executing\r\n{sqlStatement}\r\n{parameters}\r\n{ex.Message}";
            }
            else
            {
                errorMessage = ex.Message;
            }

            // try to create a more specialized exception
            if (ex is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 547: // violation of constraint
                        return new ConstraintViolationException(errorMessage, sqlEx);
                    case 2601: // violation of unique index
                        return new UniqueIndexViolationException(errorMessage, sqlEx);
                    case 2627: // violation of unique constraint (e.g. primary key, unique)
                        return new UniqueConstraintViolationException(errorMessage, sqlEx);
                }
            }
            return new Exception(errorMessage, ex);
        }

        
    }
}
