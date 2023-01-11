using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Provides methods to generate instance of <see cref="SqlStatement{TMapped}"/>.
    /// </summary>
    public class SqlStatementGenerator
    {
        /// <summary>
        /// Generates an instance of <see cref="SqlStatement{TMapped}"/>.
        /// </summary>
        /// <typeparam name="TMapped"></typeparam>        
        /// <param name="statementType"></param>
        /// <param name="resultType"></param>
        /// <param name="map"></param>
        /// <param name="query"></param>        
        /// <returns></returns>
        /// <exception cref="InvalidQueryException"></exception>
        public SqlStatement<TMapped> GenerateSqlStatment<TMapped>(DMLStatementType statementType, SqlStatementResultCardinality resultType, DataMap<TMapped> map, DataQuery<TMapped> query) where TMapped : new()
        {
            return GenerateSqlStatment(statementType, resultType, map, query, default, false);
        }

        /// <summary>
        /// Generates an instance of <see cref="SqlStatement{TMapped}"/>. Use this overload to pass an entity to insert or update.
        /// </summary>
        /// <typeparam name="TMapped"></typeparam>        
        /// <param name="statementType"></param>
        /// <param name="resultType"></param>
        /// <param name="map"></param>
        /// <param name="query"></param>        
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert"></param>
        /// <returns></returns>
        /// <exception cref="InvalidQueryException"></exception>
        public SqlStatement<TMapped> GenerateSqlStatment<TMapped>(DMLStatementType statementType, SqlStatementResultCardinality resultType, DataMap<TMapped> map, DataQuery<TMapped> query, 
            TMapped entity, bool isIdentityInsert) where TMapped: new()
        {
            //validate if the supplied mapping has everything needed to generate queries
            if (statementType != DMLStatementType.CustomQuery)
                map.ValidateMappingForGeneratedQueries();

            var result = new SqlStatement<TMapped>(statementType, resultType) { CustomSqlStatement = query.SqlQuery };
            
            //create the DML clause and optionally the order by clause of the query            
            switch (statementType)
            {
                case DMLStatementType.Select:
                    ApplySelectToStatement(result, map, query);                                        
                    break;
                case DMLStatementType.Insert:
                    ApplyInsertToStatement(result, map, query, entity, isIdentityInsert);                                        
                    break;
                case DMLStatementType.Update:
                    ApplyUpdateToStatement(result, map, query, entity);                    
                    break;
                case DMLStatementType.Delete:
                    ApplyDeleteToStatement(result, query);
                    break;
                case DMLStatementType.CustomQuery:                                        
                    AddWhereClauseToStatement(result, query);
                    break;
                case DMLStatementType.InsertOrUpdate:
                    ApplyInsertOrUpdateToStatement(result, map, query, entity, isIdentityInsert);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //set the from clause
            result.FromClause = $"FROM {map.TableName}";

            return result;
        }

        private SqlStatement<T> ApplyDeleteToStatement<T>(SqlStatement<T> statement, DataQuery<T> query) where T : new()
        {
            statement.DmlClause = "DELETE ";
            AddWhereClauseToStatement(statement, query);
            return statement;
        }

        private SqlStatement<T> ApplyInsertOrUpdateToStatement<T>(SqlStatement<T> statement, DataMap<T> map, DataQuery<T> query, T entity, bool isIdentityInsert) where T : new()
        {
            // this generates two seperate statements which need to be merged into one statement which uses an IF EXIST / ELSE
            // generate insert
            var insertStatement = GenerateSqlStatment(DMLStatementType.Insert, SqlStatementResultCardinality.SingleRow, map, query, entity, isIdentityInsert);
            // generate update
            var updateStatement = GenerateSqlStatment(DMLStatementType.Update, SqlStatementResultCardinality.None, map, query, entity, isIdentityInsert);

            // generate custom insert or update statement
            statement.CustomSqlStatement = $@"
IF EXISTS(SELECT * FROM {map.TableName} {updateStatement.WhereClause})
BEGIN
{updateStatement.ToString()}
END
ELSE
BEGIN
{insertStatement.ToString()}
END";
            
            // add the parameters from update and insert statements
            statement.Parameters.AddRange(updateStatement.Parameters);
            statement.Parameters.AddRange(insertStatement.Parameters);

            return statement;
        }

        private SqlStatement<T> ApplyUpdateToStatement<T>(SqlStatement<T> statement, DataMap map, DataQuery<T> query, T entity) where T : new()
        {
            statement.DmlClause = $"UPDATE {map.TableName}";
            // generate the set clause for all columns that are not readonly, and add the parameter to the statement
            var columnsToUpdate = map.Items.Where(x => x.IsReadOnly == false && x.IsIdentity == false).ToList();
            var setClauseColumns = new List<string>();
            for (int i = 0; i < columnsToUpdate.Count; i++)
            {
                var column = columnsToUpdate[i];
                string parameterName = $"@u{i}";
                var value = ReflectionHelper.GetMemberValue(column.MemberInfoTree, entity);
                statement.Parameters.Add(new SqlStatementParameter(parameterName, value, column.SqlType, column.Length));
                setClauseColumns.Add($"{column.Column} = {parameterName}");
            }
            statement.UpdateSetClause = $"SET {string.Join(",", setClauseColumns)}";

            // add where clause
            AddWhereClauseToStatement(statement, query);

            return statement;
        }

        private SqlStatement<T> ApplySelectToStatement<T>(SqlStatement<T> statement, DataMap map, DataQuery<T> query) where T: new()
        {
            // set opening statement
            statement.DmlClause = "SELECT ";
            if (statement.ResultCardinality == SqlStatementResultCardinality.SingleRow)
                statement.DmlClause += "TOP(1) ";
            else if (query?.MaxResults != null)
                statement.DmlClause += $"TOP({query.MaxResults}) ";

            // generate the column list, ie. MyColumn1, MyColumn2, MyColumn3 + MyColumn4 AS MyAlias
            statement.DmlClause += string.Join(",", map.Items.Select(x => x.ColumnSelectListName));

            // set order by from query
            if (query != null)
                statement.OrderByClause = query.OrderBy;

            // add offset to order by if paging is supplied
            if (query?.Paging != null && query?.Paging?.NumberOfRows > 0)
            {
                statement.AddPagingRowCountStatement = true;

                if (string.IsNullOrWhiteSpace(statement.OrderByClause))
                {
                    // if offset is used, it always needs an order by clause. create one if none supplied.
                    var primaryKeyColumns = map.GetPrimaryKeyColumns();
                    if (primaryKeyColumns.Count > 0)
                        statement.OrderByClause = "ORDER BY " + string.Join(",", primaryKeyColumns.Select(x => x.Column));
                    else
                        throw new InvalidQueryException("Cannot apply paging to an unordered SQL SELECT statement. Add an order by clause or map a primary key.");
                }
                statement.OrderByClause += $" OFFSET {query.Paging.PageIndex * query.Paging.NumberOfRows} ROWS FETCH NEXT {query.Paging.NumberOfRows} ROWS ONLY";
            }

            // add where clause
            AddWhereClauseToStatement(statement, query);

            return statement;
        }

        private SqlStatement<T> ApplyInsertToStatement<T>(SqlStatement<T> statement, DataMap<T> map, DataQuery<T> query, T entity, bool isIdentityInsert) where T : new()
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
                statement.InsertIntoClause += $" ({string.Join(",", insertColumns.Select(x => x.Column))})";
                //add a parameter for each column
                for (int i = 0; i < insertColumns.Count; i++)
                {
                    var column = insertColumns[i];
                    string parameterName = $"@i{i}";
                    var value = ReflectionHelper.GetMemberValue(column.MemberInfoTree, entity);
                    statement.Parameters.Add(new SqlStatementParameter(parameterName, value, column.SqlType, column.Length));
                }
                //create column list
                statement.InsertValuesClause = $"VALUES ({string.Join(",", statement.Parameters.Select(x => x.Name))})";

                //add an output clause if we need to retrieve the identity value                        
                if (!isIdentityInsert)
                {
                    var identityColumn = map.Items.FirstOrDefault(x => x.IsIdentity);
                    if (identityColumn != null)
                        statement.OutputClause = $"OUTPUT inserted.{identityColumn.Column}";
                }
            }

            // add where clause
            AddWhereClauseToStatement(statement, query);

            return statement;
        }
        
        /// <summary>
        /// Sets <see cref="SqlStatement{TMapped}.WhereClause"/> and <see cref="SqlStatement{TMapped}.Parameters"/> based on values supplied in <paramref name="query"/>.
        /// </summary>        
        /// <param name="sqlStament"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private SqlStatement<T> AddWhereClauseToStatement<T>(SqlStatement<T> sqlStament, DataQuery<T> query) where T: new()
        {
            // get custom sql parameters from query and add to result
            if (query?.SqlParameters != null)
            {
                foreach (var p in query.SqlParameters)
                {
                    sqlStament.Parameters.Add(new SqlStatementParameter(p.ParameterName, p.Value, p.SqlDbType, 0, p.TypeName));                    
                }
            }

            // if the query does not contain a where clause, we are done
            if (query?.WhereClause?.Any() != true)
            {
                return sqlStament;
            }

            var sb = new StringBuilder("WHERE ");
            // generate a sql text where predicate for each predicate in the query's where clause
            for (int i = 0; i < query.WhereClause.Count; i++)
            {
                WhereCondition predicate = query.WhereClause[i];

                string parameterName = $"@C{i}";

                // if custom sql was provided for this predicate, add that to the where clause. otherwise, build the sql for the where clause
                if (!string.IsNullOrWhiteSpace(predicate.SqlText))
                {
                    sb.Append(predicate.SqlText);
                }
                else
                {
                    switch (predicate.CompareType)
                    {
                        case ComparisonOperator.Equals:
                            // if we need to compare to a NULL value, use an 'IS NULL' predicate
                            if (predicate.Value == null)
                            {
                                sb.Append($"{predicate.Column} IS NULL");
                            }
                            else
                            {
                                sb.Append($"{predicate.Column} = {parameterName}");
                                sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));

                            }
                            break;
                        case ComparisonOperator.NotEqualTo:
                            // if we need to compare to a NULL value, use an 'IS NOT NULL' predicate
                            if (predicate.Value == null)
                            {
                                sb.Append($"{predicate.Column} IS NOT NULL");
                            }
                            else
                            {
                                sb.Append($"{predicate.Column} <> {parameterName}");
                                sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            }
                            break;
                        case ComparisonOperator.Like:
                            sb.Append($"{predicate.Column} LIKE {parameterName}");
                            sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                        case ComparisonOperator.In:
                            if (predicate.Value is IEnumerable items)
                            {
                                // create a unique parameter for each item in the 'IN' predicate
                                var inParams = new List<string>();
                                int j = 0;
                                if (items != null)
                                {
                                    foreach (var item in items)
                                    {
                                        string inParam = $"{parameterName}_{j}";

                                        sqlStament.Parameters.Add(new SqlStatementParameter(inParam, item, predicate.SqlType, predicate.Length));
                                        inParams.Add(inParam);

                                        j++;
                                    }
                                }
                                // if there are items in the collection, add a predicate to the where in clause. 
                                // if not, add a predicate that always evaluates to false, because no row will match the empty values
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
                            sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                        case ComparisonOperator.GreaterThanOrEquals:
                            sb.Append($"{predicate.Column} >= {parameterName}");
                            sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                        case ComparisonOperator.LessThan:
                            sb.Append($"{predicate.Column} < {parameterName}");
                            sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                        case ComparisonOperator.LessThanOrEquals:
                            sb.Append($"{predicate.Column} <= {parameterName}");
                            sqlStament.Parameters.Add(new SqlStatementParameter(parameterName, predicate.Value, predicate.SqlType, predicate.Length));
                            break;
                    }
                }

                // add 'AND' if this is not the last condition
                if(i + 1 < query.WhereClause.Count)
                {
                    sb.Append(" AND ");
                }
            }

            // add plain text where clause to sql statement
            sqlStament.WhereClause = sb.ToString();
            
            return sqlStament;
        }
    }
}
