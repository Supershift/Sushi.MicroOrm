using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{   
    /// <summary>
    /// Operators that test whether two expressions are the same. The operators are used to construct predicates for a WHERE search condition.
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>
        /// Compares the equality of two expressions, i.e. WHERE {column} = {value}, WHERE {column} IS NULL. 
        /// </summary>        
        Equals,
        /// <summary>
        /// Compares the equality of two expressions, resulting to TRUE if the left operand is not equal to the right operand, i.e. WHERE {column} != {value}, WHERE {column} IS NOT NULL. 
        /// </summary>        
        NotEqualTo,
        /// <summary>
        /// Determines whether a specific character string matches a specified pattern, i.e. WHERE {column} LIKE {pattern}. 
        /// </summary>
        Like,
        /// <summary>
        /// Determines whether a specified value matches any value in a subquery or a list, i.e. WHERE {column} IN ({value1},{value2},{value3}).
        /// </summary>
        In,
        /// <summary>
        /// Compares two expressions for greater than, i.e. WHERE {column} &gt; {value}
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Compares two expressions for greater than or equal, i.e. WHERE {column} &gt;= {value}
        /// </summary>
        GreaterThanOrEquals,
        /// <summary>
        /// Compares two expressions for less than, i.e. WHERE {column} &lt; {value}
        /// </summary>
        LessThan,
        /// <summary>
        /// Compares two expressions for less than or eqaul to, i.e. WHERE {column} &lt;= {value}
        /// </summary>
        LessThanOrEquals
    }
}
