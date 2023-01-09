using System;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Linq;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Represents a condition in a where clause
    /// </summary>
    public class WhereCondition
    {
        /// <summary>
        /// Gets or sets a custom SQL statement to use as condition.
        /// </summary>
        public string SqlText { get; set; }
        /// <summary>
        /// Gets or sets the name of the column to which this predicate applies.
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// Gets or sets the value to which <see cref="Column"/> is tested.
        /// </summary>
        public object Value { get; set; }        
        /// <summary>
        /// Get or sets the <see cref="SqlDbType"/> of <see cref="Column"/>. This will also be the <see cref="SqlDbType"/> of the parameter generated to define <see cref="Value"/>.
        /// </summary>
        public SqlDbType SqlType { get; set; }
        /// <summary>
        /// Get or sets the length of the paramater generated to define <see cref="Value"/>.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="ComparisonOperator"/> used to compare <see cref="Column"/> and <see cref="Value"/>.
        /// </summary>
        public ComparisonOperator CompareType { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="WhereCondition"/> using the specified SQL statement as predicate.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        public WhereCondition(string sqlText)
        {
            SqlText = sqlText;
        }                

        /// <summary>
        /// Initializes a new instance of <see cref="WhereCondition"/> where the predicate is built using the specified <paramref name="column"/> and <paramref name="value"/>.
        /// </summary>
        public WhereCondition(string column, SqlDbType type, object value, ComparisonOperator comparisonOperator)
            : this(column, type, value, 0, comparisonOperator) { }

        /// <summary>
        /// Initializes a new instance of <see cref="WhereCondition"/> where the predicate is built using the specified <paramref name="column"/> and <paramref name="value"/>.
        /// </summary>
        public WhereCondition(string column, SqlDbType type, object value, int length, ComparisonOperator comparisonOperator)
        {
            Column = column;
            SqlType = type;         
            Value = value;
            Length = length;
            CompareType = comparisonOperator;            
        }
    }
}
