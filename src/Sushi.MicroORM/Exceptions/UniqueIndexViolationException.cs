using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// The exception that is thrown when trying to insert a row which violates a unique index.
    /// </summary>
    public class UniqueIndexViolationException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="UniqueIndexViolationException"/>.
        /// </summary>        
        public UniqueIndexViolationException(string message, DbException dbException) : base(message, dbException)
        {
            
        }
    }
}
