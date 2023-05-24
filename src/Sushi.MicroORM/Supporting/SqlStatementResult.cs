﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Represents the response generated by executing a <see cref="SqlStatement{TResult}"/>.
    /// </summary>    
    /// <typeparam name="TResult"></typeparam>
    public class SqlStatementResult<TResult> 
    {   
        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementResult{TResult}"/> with <see cref="ResultCardinality"/> <see cref="SqlStatementResultCardinality.None"/>. 
        /// </summary>
        public SqlStatementResult()
        {
            ResultCardinality = SqlStatementResultCardinality.None;
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementResult{TResult}"/> with type <see cref="SqlStatementResultCardinality.MultipleRows"/>.
        /// </summary>
        /// <param name="results"></param>
        public SqlStatementResult(QueryListResult<TResult?> results)
        {
            ResultCardinality = SqlStatementResultCardinality.MultipleRows;
            MultipleResults = results;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementResult{TResult}"/> with type <see cref="SqlStatementResultCardinality.MultipleRows"/>.
        /// </summary>        
        public SqlStatementResult(QueryListResult<TResult?> results, int? totalNumberOfRows) : this(results)
        {
            TotalNumberOfRows = totalNumberOfRows;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SqlStatementResult{TResult}"/> with type set to <see cref="SqlStatementResultCardinality.SingleRow"/>.
        /// </summary>                
        public SqlStatementResult(TResult? result)
        {
            ResultCardinality = SqlStatementResultCardinality.SingleRow;
            SingleResult = result;
        }

        /// <summary>
        /// Gets a value indicating the cardinality of the result returned by the sql statement. If <see cref="SqlStatementResultCardinality.MultipleRows"/> the result is set on <see cref="MultipleResults"/>. 
        /// If <see cref="SqlStatementResultCardinality.SingleRow"/> the result is set on <see cref="SingleResult"/>.
        /// </summary>
        public SqlStatementResultCardinality ResultCardinality { get; private set; }
        
        /// <summary>
        /// Gets the mapped result for the sql statement if <see cref="ResultCardinality"/> is set to <see cref="SqlStatementResultCardinality.SingleRow"/>.
        /// </summary>
        public TResult? SingleResult { get; private set; }        

        /// <summary>
        /// Gets a collection of mapped results for the sql statement if <see cref="ResultCardinality"/> is set to <see cref="SqlStatementResultCardinality.MultipleRows" />.
        /// </summary>
        public QueryListResult<TResult?>? MultipleResults { get; private set; }

        /// <summary>
        /// Gets a value indicating the total number of rows for a query if paging was applied to that query.
        /// </summary>
        public int? TotalNumberOfRows { get; private set; }
    }
}
