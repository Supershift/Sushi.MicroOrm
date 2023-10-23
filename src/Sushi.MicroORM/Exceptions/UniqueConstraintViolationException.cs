using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// The exception that is thrown when trying to insert a row which violates a unique constraint, like a primary key or unique constraint.
    /// </summary>
    public class UniqueConstraintViolationException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="UniqueConstraintViolationException"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dbException"></param>
        public UniqueConstraintViolationException(string message, DbException dbException) : base(message, dbException)
        {
            
        }
    }
}
