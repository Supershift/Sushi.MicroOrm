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
        /// Creates a new instance of <see cref="PagingData"/>.
        /// </summary>
        public PagingData()
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="PagingData"/>.
        /// </summary>
        /// <param name="numberOfRows"></param>
        /// <param name="pageIndex"></param>
        public PagingData(int numberOfRows, int pageIndex)
        {
            NumberOfRows = numberOfRows;
            PageIndex = pageIndex;
        }
        
        /// <summary>
        /// Maximum number of records to retrieve per database call.
        /// </summary>        
        public int NumberOfRows { get; set; }
        
        /// <summary>
        /// Zero based page index, used as offset.
        /// </summary>
        public int PageIndex { get; set; }        
    }
}
