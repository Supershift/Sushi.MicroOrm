using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a <see cref="DataQuery{T}"/> would generate an invalid SQL query.
    /// </summary>
    public class InvalidQueryException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="InvalidQueryException"/>.
        /// </summary>
        /// <param name="message"></param>
        public InvalidQueryException(string message) : base(message)
        {
            
        }
    }
}
