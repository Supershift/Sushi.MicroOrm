using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Represents all fields necessary to perform paging.
    /// </summary>
    public class PagingData
    {   
        /// <summary>
        /// Maximum number of records to retrieve per database call.
        /// </summary>        
        public int NumberOfRows { get; set; }
        
        /// <summary>
        /// Zero based page index, used as offset.
        /// </summary>
        public int PageIndex { get; set; }
        
        /// <summary>
        /// After a query is performed the total number of rows for the supplied where clause is set here.
        /// </summary>
        [Obsolete("Use QueryListResult.TotalNumberOfRows instead")]
        public int? TotalNumberOfRows { get; set; }        
    }
}
