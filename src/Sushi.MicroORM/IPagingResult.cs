using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Defines properties for objects containing the result to an operation which suppors paging.
    /// </summary>
    public interface IPagingResult
    {
        /// <summary>
        /// After a query is performed the total number of rows for the supplied where clause is set here.
        /// </summary>
        public int? TotalNumberOfRows { get; }

        /// <summary>
        /// After a query is performed the total number of pages based on the supplied maximum number of rows is set here.
        /// </summary>
        public int? TotalNumberOfPages { get; }
    }
}
