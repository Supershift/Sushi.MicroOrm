using Sushi.MicroORM.Supporting;
using System;

namespace Sushi.MicroORM.Exceptions
{
    /// <summary>
    /// Defines an interface to handle exceptions.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Creates an exception based on the given parameters.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sqlStatement"></param>
        /// <returns></returns>
        Exception Handle(Exception ex, SqlStatement? sqlStatement);
    }
}