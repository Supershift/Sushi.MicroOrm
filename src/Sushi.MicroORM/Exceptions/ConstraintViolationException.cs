using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a statement violates a constraint, like deleting a row referenced by a foreign key constraint.
    /// </summary>
    public class ConstraintViolationException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConstraintViolationException"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dbException"></param>
        public ConstraintViolationException(string message, DbException dbException) : base(message, dbException)
        {

        }
    }
}
