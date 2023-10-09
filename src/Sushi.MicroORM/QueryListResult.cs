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
    public class QueryListResult<T> : List<T>, IPagingResult
    {
        /// <inheritdoc/>
        public int? TotalNumberOfRows { get; set; }

        /// <inheritdoc/>        
        public int? TotalNumberOfPages { get; set; }
    }
}
