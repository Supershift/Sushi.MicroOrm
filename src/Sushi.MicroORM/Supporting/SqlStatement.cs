using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    public class SqlStatement
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlStatement"/>.
        /// </summary>
        /// <param name="dmlStatement"></param>
        /// <param name="resultType"></param>
        public SqlStatement(DMLStatement dmlStatement, SqlStatementResultType resultType)
        {
            DMLStatement = dmlStatement;
            ResultType = resultType;
        }

        public DMLStatement DMLStatement { get; protected set; }

        public SqlStatementResultType ResultType { get; protected set; }

        /// <summary>
        /// Gets or sets the DML clause of the statement, ie. SELECT MyColumn, UPDATE MyTable, etc.
        /// </summary>
        public string DmlClause { get; set; }

        public string InsertIntoClause { get; set; }
        public string InsertValueClause { get; set; }
        public string UpdateSetClause { get; set; }
        /// <summary>
        /// Gets or sets the output clause of the statement, ie. OUTPUT Inserted.MyColumn
        /// </summary>
        public string OutputClause { get; set; }


        /// <summary>
        /// Gets or sets the where clause of the statment, ie. WHERE MyColumn = @myParameter
        /// </summary>
        public string WhereClause { get; set; }

        /// <summary>
        /// Gets or sets the from clause of the statement, ie. FROM MyTable
        /// </summary>
        public string FromClause { get; set; }

        /// <summary>
        /// Gets or sets the order by clause of the statement, ie. ORDER BY MyColumn1 ASC, MyColumn2 DESC
        /// </summary>
        public string OrderByClause { get; set; }

        public string CustomSqlStatement { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="StatementParameter"/> objects describing the parameters used in the statement.
        /// </summary>
        public List<StatementParameter> Parameters { get; } = new List<StatementParameter>();

        /// <summary>
        /// Gets or sets a value indicating if a second statement must be added which will retrieve the total number of rows for the resultset defined by <see cref="FromClause"/> and <see cref="WhereClause"/>.
        /// </summary>
        public bool AddPagingRowCountStatement { get; set; }

        /// <summary>
        /// Generates a sql statement based on the clauses.
        /// </summary>
        /// <returns></returns>
        public string GenerateSqlStatement()
        {
            if (!string.IsNullOrWhiteSpace(CustomSqlStatement))
                return CustomSqlStatement;
            else
            {
                string result;
                switch (DMLStatement)
                {
                    case DMLStatement.Insert:
                        result = $"{DmlClause}\r\n{InsertIntoClause}\r\n{OutputClause}\r\n{InsertValueClause}";
                        break;
                    case DMLStatement.Update:
                        result = $"{DmlClause}\r\n{UpdateSetClause}\r\n{OutputClause}\r\n{FromClause}\r\n{WhereClause}";
                        break;
                    default:
                        result = $"{DmlClause}\r\n{FromClause}\r\n{WhereClause}\r\n{OrderByClause}";
                        if (AddPagingRowCountStatement)
                            result += "\r\n" + GeneratePagingRowCountSqlStatement();
                        break;
                }

                return result;
            }
        }

        /// <summary>
        /// Generates a sql statement which counts the total number of rows for the resultset defined by <see cref="FromClause"/> and <see cref="WhereClause"/>.
        /// </summary>
        /// <returns></returns>
        private string GeneratePagingRowCountSqlStatement()
        {
            string result = $"SELECT COUNT(*)\r\n{FromClause}\r\n{WhereClause}";
            return result;
        }
    }
}
