using Sushi.MicroORM.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    public enum DMLStatement
    {
        Select,
        Update,
        Insert,
        Delete,
        InsertOrUpdate,
        CustomQuery
    }

    

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
        /// Gets or sets the DML clause of the statement, ie. SELECT MyColumn, UPDATE MyTable SET MyColumn = @myParameter, etc.
        /// </summary>
        public string DmlClause { get; set; }
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

        /// <summary>
        /// Gets a collection of <see cref="StatementParameter"/> objects describing the parameters used in the statement.
        /// </summary>
        public List<StatementParameter> Parameters { get; } = new List<StatementParameter>();
        /// <summary>
        /// Generates a sql statement based on the clauses.
        /// </summary>
        /// <returns></returns>
        public string GenerateSqlStatement()
        {
            string result = $"{DmlClause}\r\n{FromClause}\r\n{WhereClause}\r\n{OrderByClause}";
            return result;
        }
    }

    public class StatementParameter
    {
        public StatementParameter(string name, object value, SqlDbType type, int length) : this(name, value, type, length, null) { }        

        public StatementParameter(string name, object value, SqlDbType type, int length, string typeName)
        {
            Name = name;
            Value = value;
            Type = type;
            Length = length;
            TypeName = typeName;
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public SqlDbType Type { get; set; }
        public int Length { get; set; }
        public string TypeName { get; set; }
    }

    public static class SqlStatementGenerator
    {
        public static SqlStatement GenerateSqlStatment<T>(DMLStatement statementType, SqlStatementResultType resultType, DataMap<T> map, DataFilter<T> filter, string customQuery) where T: new()
        {
            var result = new SqlStatement(statementType, resultType);
            string dmlClause;
            string orderByClause = null;
            //create the DML clause and optionally the order by clause of the query            
            switch (statementType)
            {
                case DMLStatement.Select:

                    dmlClause = "SELECT ";
                    if (resultType == SqlStatementResultType.Single)
                        dmlClause += "TOP(1) ";
                    //generate the column list, ie. MyColumn1, MyColumn2, MyColumn3 + MyColumn4 AS MyAlias
                    dmlClause += string.Join(",", map.Items.Select(x => x.ColumnSelectListName));

                    //set order by from filter
                    if(filter != null)
                        orderByClause = filter.OrderBy;

                    break;
                default:
                    throw new NotImplementedException();
            }
            result.DmlClause = dmlClause;
            result.OrderByClause = orderByClause;

            //set the from clause
            result.FromClause = $"FROM {map.TableName}";

            //add the where clause to the query 
            AddWhereClauseToStatement(result, filter);

            return result;
        }
        
        /// <summary>
        /// Sets <see cref="SqlStatement.Where"/> and <see cref="SqlStatement.Parameters"/> based on values supplied in <paramref name="filter"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static SqlStatement AddWhereClauseToStatement<T>(SqlStatement query, DataFilter<T> filter) where T: new()
        {
            var whereClause = filter?.WhereClause;

            //get custom sql parameters from filter and add to result
            if (filter?.SqlParameters != null)
            {
                foreach (SqlParameter p in filter.SqlParameters)
                {
                    query.Parameters.Add(new StatementParameter(p.ParameterName, p.Value, p.SqlDbType, 0, p.TypeName));                    
                }
            }

            if (whereClause == null || whereClause.Count == 0) return query;
            
            bool orGroupIsSet = false;

            var sb = new StringBuilder("WHERE ");
            //generate a sql text where predicate for each predicate in the filter's where clause
            for(int i=0;i<whereClause.Count; i++) 
            {
                WhereCondition predicate = whereClause[i];

                string parameterName = $"@C{i}";

                //add opening paranthesis to seperate predicates based on where condition operator
                WhereCondition nextcolumn = null;

                while (nextcolumn == null)
                {
                    if (whereClause.Count > i + 1)
                    {
                        nextcolumn = whereClause[i + 1];

                        if (predicate.ConnectType == WhereConditionOperator.And && nextcolumn.ConnectType == WhereConditionOperator.Or)
                        {
                            orGroupIsSet = true;
                            sb.Append('(');
                        }
                    }
                    else
                        break;
                }

                //if custom sql was provided for this predicate, add that to the where clause. otherwise, build the sql for the where clause
                if (!string.IsNullOrWhiteSpace(predicate.SqlText))
                {
                    sb.Append(predicate.SqlText);
                }
                else
                {
                    switch (predicate.CompareType)
                    {
                        case ComparisonOperator.Equals:
                            //if we need to compare to a NULL value, use an 'IS NULL' predicate
                            if (predicate.Value == null)
                            {
                                sb.Append($"{predicate.Column} IS NULL");
                            }
                            else
                            {
                                sb.Append($"{predicate.Column} = {parameterName}");
                                query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                                
                            }
                            break;
                        case ComparisonOperator.NotEqualTo:
                            //if we need to compare to a NULL value, use an 'IS NOT NULL' predicate
                            if (predicate.Value == null)
                            {
                                sb.Append($"{predicate.Column} IS NOT NULL");
                            }
                            else
                            {
                                sb.Append($"{predicate.Column} <> {parameterName}");
                                query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            }
                            break;
                        case ComparisonOperator.Like:
                            sb.Append($"{predicate.Column} LIKE {parameterName}");                            
                            query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                        case ComparisonOperator.In:
                            if (predicate.Value is IEnumerable items)
                            {
                                //create a unique parameter for each item in the 'IN' predicate
                                var inParams = new List<string>();
                                int j = 0;
                                if (items != null)
                                {
                                    foreach (var item in items)
                                    {
                                        string inParam = $"{parameterName}_{j}";

                                        query.Parameters.Add(new StatementParameter(inParam, item, predicate.SqlType, predicate.Length));
                                        inParams.Add(inParam);

                                        j++;
                                    }
                                }
                                //if there are items in the collection, add a predicate to the where in clause. 
                                //if not, add a predicate that always evaluates to false, because no row will match the empty values
                                if (inParams.Count > 0)
                                    sb.Append($"{predicate.Column} IN ({string.Join(",", inParams)})");
                                else
                                    sb.Append("1 = 0");
                            }
                            else
                            {
                                throw new Exception($"Cannot build WHERE clause. When using {nameof(ComparisonOperator.In)}, supply an IEnumerable as value.");
                            }
                            break;
                        case ComparisonOperator.GreaterThan:
                            sb.Append($"{predicate.Column} > {parameterName}");                            
                            query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
                            break;
                        case ComparisonOperator.GreaterThanOrEquals:
                            sb.Append($"{predicate.Column} >= {parameterName}");                            
                            query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
                            break;
                        case ComparisonOperator.LessThan:
                            sb.Append($"{predicate.Column} < {parameterName}");                            
                            query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                            
                            break;
                        case ComparisonOperator.LessThanOrEquals:
                            sb.Append($"{predicate.Column} <= {parameterName}");
                            query.Parameters.Add(new StatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
                            break;
                    }
                }

                //add closing paranthesis to seperate predicates based on where condition operator
                if (nextcolumn != null)
                {
                    if (nextcolumn.ConnectType == WhereConditionOperator.And)
                    {
                        if (orGroupIsSet)
                        {
                            orGroupIsSet = false;
                            sb.Append(") AND ");
                        }
                        else
                            sb.Append(" AND ");
                    }
                    else if (nextcolumn.ConnectType == WhereConditionOperator.Or || nextcolumn.ConnectType == WhereConditionOperator.OrUngrouped)
                    {
                        sb.Append(" OR ");
                    }
                }
                i++;
            }

            //add last closing paranthesis
            if (orGroupIsSet)
                sb.Append(")");

            //add where clause to query
            query.WhereClause = sb.ToString();
            
            return query;
        }
    }
}
