using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Data;

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Windows.Input;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Exceptions;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Executes <see cref="SqlStatement"/>.
    /// </summary>
    public class SqlExecuter
    {   
        private readonly ResultMapper _resultMapper;
        private readonly IExceptionHandler _exceptionHandler;
        /// <summary>
        /// Creates a new instance of <see cref="SqlExecuter"/>.
        /// </summary>
        public SqlExecuter(ResultMapper resultMapper, IExceptionHandler exceptionHandler)
        {
            _resultMapper = resultMapper;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Adds a parameter to the SQL command.
        /// </summary>        
        private void SetParameter(SqlCommand command, string name, object? itemvalue, SqlDbType type, int length, ParameterDirection direction, string? typeName)
        {
            // if we already have the parameter, ignore the call
            if (command.Parameters.Contains(name)) return;

            // create the parameter and add it to the command's collection of parameters
            var parameter = command.Parameters.Add(name, type, length);

            parameter.Direction = direction;

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                parameter.TypeName = typeName;
            }

            // set the value. if supplied value is null, set the DB equivalent
            if (itemvalue == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = itemvalue;                
            }
        }

        /// <summary>
        /// Executes the <paramref name="sqlStatement"/> and adds the result to a <see cref="SqlStatementResult{TResult}"/>.
        /// </summary>        
        public async Task<SqlStatementResult<TResult>> ExecuteAsync<T, TResult>(SqlStatement sqlStatement, string connectionString, int? commandTimeout, 
            DataMap<T> map, CancellationToken cancellationToken)
        {
            SqlStatementResult<TResult> result;

            // create query text from statement
            var query = sqlStatement.ToString();

            try
            {
                // open connection
                using var connection = new SqlConnection(connectionString);

                // create command
                using var command = new SqlCommand(query, connection);

                // set the commands time out
                if (commandTimeout.HasValue)
                    command.CommandTimeout = commandTimeout.Value;

                // add parameters from statement
                foreach (var parameter in sqlStatement.Parameters)
                {
                    SetParameter(command, parameter.Name, parameter.Value, parameter.Type, parameter.Length, ParameterDirection.Input, parameter.TypeName);
                }

                // open connection
                await connection.OpenAsync().ConfigureAwait(false);

                // execute the command                
                SqlDataReader? reader = null;

                try
                {
                    switch (sqlStatement.ResultCardinality)
                    {
                        case SqlStatementResultCardinality.SingleRow:
                            // execute the command, which will return a reader
                            reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                            // if the result type of the statement is the same, or inherits, the mapped type T, use the map to create a result object
                            if (typeof(TResult) == typeof(T) || typeof(TResult).IsSubclassOf(typeof(T)))
                            {
                                var singleResult = await _resultMapper.MapToSingleResultAsync(reader, map, cancellationToken).ConfigureAwait(false);
                                result = new SqlStatementResult<TResult>((TResult?)(object?)singleResult);
                            }
                            else
                            {
                                // create a single (scalar) result
                                var scalarResult = await _resultMapper.MapToSingleResultScalarAsync<TResult>(reader, cancellationToken).ConfigureAwait(false);
                                result = new SqlStatementResult<TResult>(scalarResult);
                            }
                            break;
                        case SqlStatementResultCardinality.MultipleRows:
                            // execute the command, which will return a reader
                            reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                            // if the result type of the statement is the same, or inherits, the mapped type T, use the map to create a result object
                            if (typeof(TResult) == typeof(T) || typeof(TResult).IsSubclassOf(typeof(T)))
                            {
                                // map the contents of the reader to a result
                                var multipleResults = await _resultMapper.MapToMultipleResultsAsync(reader, map, cancellationToken).ConfigureAwait(false);
                                // cast to TResult
                                var castedResults = new QueryListResult<TResult?>();
                                foreach (var singleResult in multipleResults)
                                {
                                    castedResults.Add((TResult?)(object?)singleResult);
                                }

                                // check if there is a 2nd result set with total number of rows for paging
                                int? numberOfRows = null;

                                if (reader.NextResult())
                                {
                                    numberOfRows = await _resultMapper.MapToSingleResultScalarAsync<int?>(reader, cancellationToken).ConfigureAwait(false);
                                }

                                result = new SqlStatementResult<TResult>(castedResults, numberOfRows);
                            }
                            else
                            {
                                var multipleResults = await _resultMapper.MapToMultipleResultsScalarAsync<TResult>(reader, cancellationToken).ConfigureAwait(false);
                                // check if there is a 2nd result set with total number of rows for paging
                                int? numberOfRows = null;

                                if (reader.NextResult())
                                {
                                    numberOfRows = await _resultMapper.MapToSingleResultScalarAsync<int?>(reader, cancellationToken).ConfigureAwait(false);
                                }

                                result = new SqlStatementResult<TResult>(multipleResults, numberOfRows);
                            }
                            break;
                        case SqlStatementResultCardinality.None:
                            // execute the command
                            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                            result = new SqlStatementResult<TResult>();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        await reader.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) { throw; }            
            catch (Exception ex)
            {
                throw _exceptionHandler.Handle(ex, sqlStatement);
            }

            return result;
        }

        
    }
}
