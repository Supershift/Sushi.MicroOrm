using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Represents the result to a query which returned multiple entities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryListResult<T> : List<T> 
    {
        /// <summary>
        /// After a query is performed the total number of rows for the supplied where clause is set here.
        /// </summary>
        public int? TotalNumberOfRows { get; set; }

        /// <summary>
        /// After a query is performed the total number of pages based on the supplied maximum number of rows is set here.
        /// </summary>
        public int? TotalNumberOfPages { get; set; }
    }
}
