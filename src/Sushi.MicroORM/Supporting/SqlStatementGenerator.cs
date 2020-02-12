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
    /// <summary>
    /// Provides methods to generate instance of <see cref="SqlStatement{TMapped, TResult}"/>.
    /// </summary>
    public static class SqlStatementGenerator
    {
        /// <summary>
        /// Generates an instance of <see cref="SqlStatement{TMapped, TResult}"/>.
        /// </summary>
        /// <typeparam name="TMapped"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="statementType"></param>
        /// <param name="resultType"></param>
        /// <param name="map"></param>
        /// <param name="filter"></param>
        /// <param name="customQuery"></param>
        /// <returns></returns>
        public static SqlStatement<TMapped> GenerateSqlStatment<TMapped>(DMLStatementType statementType, SqlStatementResultCardinality resultType, DataMap<TMapped> map, DataFilter<TMapped> filter, string customQuery) where TMapped : new()
        {
            return GenerateSqlStatment<TMapped>(statementType, resultType, map, filter, customQuery, default(TMapped), false);
        }

        /// <summary>
        /// Generates an instance of <see cref="SqlStatement{TMapped, TResult}"/>. Use this overload to provide an entity to insert or update.
        /// </summary>
        /// <typeparam name="TMapped"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="statementType"></param>
        /// <param name="resultType"></param>
        /// <param name="map"></param>
        /// <param name="filter"></param>
        /// <param name="customQuery"></param>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert"></param>
        /// <returns></returns>
        public static SqlStatement<TMapped> GenerateSqlStatment<TMapped>(DMLStatementType statementType, SqlStatementResultCardinality resultType, DataMap<TMapped> map, DataFilter<TMapped> filter, string customQuery, TMapped entity, bool isIdentityInsert) where TMapped: new()
        {
            //validate if the supplied mapping has everything needed to generate queries
            if (statementType != DMLStatementType.CustomQuery)
                map.ValidateMappingForGeneratedQueries();

            var result = new SqlStatement<TMapped>(statementType, resultType) { CustomSqlStatement = customQuery };
            
            //create the DML clause and optionally the order by clause of the query            
            switch (statementType)
            {
                case DMLStatementType.Select:
                    ApplySelectToStatement(result, map, filter);                    
                    AddWhereClauseToStatement(result, filter);
                    break;
                case DMLStatementType.Insert:
                    ApplyInsertToStatement(result, map, entity, isIdentityInsert);                    
                    AddWhereClauseToStatement(result, filter);
                    break;
                case DMLStatementType.Update:
                    result.DmlClause = $"UPDATE {map.TableName}";
                    //generate the set clause for all columns that are not readonly, and add the parameter to the statement
                    var columnsToUpdate = map.Items.Where(x => x.IsReadOnly == false && x.IsIdentity == false).ToList();
                    var setClauseColumns = new List<string>();
                    for(int i =0;i < columnsToUpdate.Count;i++)
                    {
                        var column = columnsToUpdate[i];
                        string parameterName = $"@u{i}";
                        var value = ReflectionHelper.GetMemberValue(column.MemberInfoTree, entity);
                        result.Parameters.Add(new SqlStatementParameter(parameterName, value, column.SqlType, column.Length));
                        setClauseColumns.Add($"{column.Column} = {parameterName}");
                    }
                    result.UpdateSetClause = $"SET {string.Join(",", setClauseColumns)}";                    
                    AddWhereClauseToStatement(result, filter);
                    break;
                case DMLStatementType.Delete:
                    result.DmlClause = "DELETE ";                    
                    AddWhereClauseToStatement(result, filter);
                    break;
                case DMLStatementType.CustomQuery:                                        
                    AddWhereClauseToStatement(result, filter);
                    break;
                case DMLStatementType.InsertOrUpdate:
                    //this generates two seperate statements which need to be merged into one statement which uses an IF EXIST / ELSE
                    //generate insert
                    var insertStatement = GenerateSqlStatment<TMapped>(DMLStatementType.Insert, SqlStatementResultCardinality.SingleRow, map, filter, null, entity, isIdentityInsert);
                    //generate update
                    var updateStatement = GenerateSqlStatment<TMapped>(DMLStatementType.Update, SqlStatementResultCardinality.None, map, filter, null, entity, isIdentityInsert);

                    //generate custom insert or update statement
                    result.CustomSqlStatement = $@"
IF EXISTS(SELECT * FROM {map.TableName} {updateStatement.WhereClause})
BEGIN
{updateStatement.GenerateSqlStatement()}
END
ELSE
BEGIN
{insertStatement.GenerateSqlStatement()}
END";
                    //add the parameters from update and insert statements
                    result.Parameters.AddRange(updateStatement.Parameters);
                    result.Parameters.AddRange(insertStatement.Parameters);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //set the from clause
            result.FromClause = $"FROM {map.TableName}";

            return result;
        }

        private static SqlStatement<T> ApplySelectToStatement<T>(SqlStatement<T> statement, DataMap map, DataFilter<T> filter) where T: new()
        {
            //set opening statement
            statement.DmlClause = "SELECT ";
            if (statement.ResultCardinality == SqlStatementResultCardinality.SingleRow)
                statement.DmlClause += "TOP(1) ";
            //generate the column list, ie. MyColumn1, MyColumn2, MyColumn3 + MyColumn4 AS MyAlias
            statement.DmlClause += string.Join(",", map.Items.Select(x => x.ColumnSelectListName));

            //set order by from filter
            if (filter != null)
                statement.OrderByClause = filter.OrderBy;

            //add offset to order by if paging is supplied
            if (filter?.Paging != null && filter?.Paging?.NumberOfRows > 0)
            {
                statement.AddPagingRowCountStatement = true;

                if (string.IsNullOrWhiteSpace(statement.OrderByClause))
                {
                    //if offset is used, it always needs an order by clause. create one if none supplied
                    var primaryKeyColumns = map.GetPrimaryKeyColumns();
                    if (primaryKeyColumns.Count > 0)
                        statement.OrderByClause = "ORDER BY " + string.Join(",", primaryKeyColumns.Select(x => x.Column));
                    else
                        throw new Exception("Cannot apply paging to an unordered SQL SELECT statement. Add an order by clause or map a primary key.");
                }
                statement.OrderByClause += $" OFFSET {filter.Paging.PageIndex * filter.Paging.NumberOfRows} ROWS FETCH NEXT {filter.Paging.NumberOfRows} ROWS ONLY";
            }
            return statement;
        }

        private static SqlStatement<T> ApplyInsertToStatement<T>(SqlStatement<T> statement, DataMap<T> map, T entity, bool isIdentityInsert) where T : new()
        {
            //generate opening statement
            statement.DmlClause = $"INSERT";
            statement.InsertIntoClause = $"INTO {map.TableName}";
            //get all columns that are not readonly or identity columns (except when this is an identityinsert)
            var insertColumns = map.Items.Where(x => x.IsReadOnly == false && (x.IsIdentity == false || isIdentityInsert)).ToList();
            if (insertColumns.Count == 0)
                statement.InsertValuesClause = "DEFAULT VALUES";
            else
            {
                //add each column to the columns list
                statement.InsertIntoClause += $"( {string.Join(",", insertColumns.Select(x => x.Column))} )";
                //add a parameter for each column
                for (int i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];
                    string parameterName = $"@i{i}";
                    var value = ReflectionHelper.GetMemberValue(column.MemberInfoTree, entity);
                    statement.Parameters.Add(new SqlStatementParameter(parameterName, value, column.SqlType, column.Length));
                }
                //create column list
                statement.InsertValuesClause = $"VALUES ( {string.Join(",", statement.Parameters.Select(x => x.Name))} )";

                //add an output clause if we need to retrieve the identity value                        
                if (!isIdentityInsert)
                {
                    var identityColumn = map.Items.FirstOrDefault(x => x.IsIdentity);
                    if (identityColumn != null)
                        statement.OutputClause = $"OUTPUT inserted.{identityColumn.Column}";
                }
            }
            return statement;
        }
        /// <summary>
        /// Sets <see cref="SqlStatement{TMapped}.WhereClause"/> and <see cref="SqlStatement{TMapped}.Parameters"/> based on values supplied in <paramref name="filter"/>.
        /// </summary>        
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static SqlStatement<T> AddWhereClauseToStatement<T>(SqlStatement<T> query, DataFilter<T> filter) where T: new()
        {
            var whereClause = filter?.WhereClause;

            //get custom sql parameters from filter and add to result
            if (filter?.SqlParameters != null)
            {
                foreach (SqlParameter p in filter.SqlParameters)
                {
                    query.Parameters.Add(new SqlStatementParameter(p.ParameterName, p.Value, p.SqlDbType, 0, p.TypeName));                    
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
                                query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                                
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
                                query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            }
                            break;
                        case ComparisonOperator.Like:
                            sb.Append($"{predicate.Column} LIKE {parameterName}");                            
                            query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
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

                                        query.Parameters.Add(new SqlStatementParameter(inParam, item, predicate.SqlType, predicate.Length));
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
                            query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
                            break;
                        case ComparisonOperator.GreaterThanOrEquals:
                            sb.Append($"{predicate.Column} >= {parameterName}");                            
                            query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
                            break;
                        case ComparisonOperator.LessThan:
                            sb.Append($"{predicate.Column} < {parameterName}");                            
                            query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                            
                            break;
                        case ComparisonOperator.LessThanOrEquals:
                            sb.Append($"{predicate.Column} <= {parameterName}");
                            query.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));                                
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
