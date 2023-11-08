﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <inheritdoc />
    public class ReadOnlyConnector<T> : IReadOnlyConnector<T>
    {
        private readonly ConnectionStringProvider _connectionStringProvider;
        private readonly SqlStatementGenerator _sqlStatementGenerator;
        private readonly SqlExecuter _sqlExecuter;        
        private readonly MicroOrmOptions _options;

        /// <summary>
        /// An object representing the mapping between <typeparamref name="T"/> and database.
        /// </summary>
        private readonly DataMap<T> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class.
        /// </summary>
        public ReadOnlyConnector(
            ConnectionStringProvider connectionStringProvider,
            IOptions<MicroOrmOptions> options,
            DataMapProvider dataMapProvider,
            SqlStatementGenerator sqlStatementGenerator,
            SqlExecuter sqlExecuter)
        {
            _connectionStringProvider = connectionStringProvider;
            _options = options.Value;
            _sqlStatementGenerator = sqlStatementGenerator;
            _sqlExecuter = sqlExecuter;
            _map = dataMapProvider.GetMapForType<T>();
        }

        /// <inheritdoc />
        public virtual DataQuery<T> CreateQuery()
        {
            return new DataQuery<T>(_map) { IsReadOnly = true };
        }

        /// <summary>
        /// Executes the <paramref name="statement"/> and returns the result generated by the execution.
        /// </summary>        
        /// <returns></returns>
        protected async Task<SqlStatementResult<TResult>> ExecuteSqlStatementAsync<TResult>(SqlStatement statement, string? connectionString, int? commandTimeout,
            bool isReadOnly, CancellationToken cancellationToken)
        {
            // get default connection string and command timeout if none supplied
            if (connectionString == null)
            {
                var candidateConnectionString = _connectionStringProvider.GetConnectionString(typeof(T));

                // use read only variant if available
                if (isReadOnly && candidateConnectionString.ReadOnly != null)
                    connectionString = candidateConnectionString.ReadOnly;
                else
                    connectionString = candidateConnectionString.Primary;
            }

            if (commandTimeout == null)
            {
                commandTimeout = _options.DefaultCommandTimeOut;
            }

            var result = await _sqlExecuter.ExecuteAsync<T, TResult>(statement, connectionString, commandTimeout, _map, cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public async Task<T?> GetFirstAsync(DataQuery<T> query)
        {
            return await GetFirstAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<T?> GetFirstAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            var statement = _sqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Select, SqlStatementResultCardinality.SingleRow, _map, query);

            // execute and get response
            var statementResult = await ExecuteSqlStatementAsync<T>(statement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);

            // return result
            return statementResult.SingleResult;
        }

        /// <inheritdoc />
        public async Task<QueryListResult<T>> GetAllAsync(DataQuery<T> query)
        {
            return await GetAllAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<QueryListResult<T>> GetAllAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            var result = new QueryListResult<T>();

            // generate the sql statement            
            var statementType = DMLStatementType.Select;
            if (!string.IsNullOrWhiteSpace(query.SqlQuery))
                statementType = DMLStatementType.CustomQuery;
            var statement = _sqlStatementGenerator.GenerateSqlStatment(statementType, SqlStatementResultCardinality.MultipleRows, _map, query);

            // execute and get response
            var statementResult = await ExecuteSqlStatementAsync<T>(statement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);

            // map result
            if (statementResult.MultipleResults != null)
            {
                foreach (var item in statementResult.MultipleResults)
                {
                    if (item != null)
                        result.Add(item);
                }
            }

            // if total number of rows is set apply it to the query's paging object
            if (query?.Paging != null && statementResult.TotalNumberOfRows.HasValue)
            {
                result.TotalNumberOfRows = statementResult.TotalNumberOfRows;
                if (query.Paging.NumberOfRows > 0)
                {
                    result.TotalNumberOfPages = (int)Math.Ceiling((double)result.TotalNumberOfRows.Value / query.Paging.NumberOfRows);
                }
            }

            // return result
            return result;
        }

        /// <inheritdoc />
        public async Task<TScalar?> ExecuteScalarAsync<TScalar>(DataQuery<T> query)
        {
            return await ExecuteScalarAsync<TScalar>(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TScalar?> ExecuteScalarAsync<TScalar>(DataQuery<T> query, CancellationToken cancellationToken)
        {
            //generate the sql statement
            var statement = _sqlStatementGenerator.GenerateSqlStatment(DMLStatementType.CustomQuery, SqlStatementResultCardinality.SingleRow, _map, query);

            //execute and get response
            var statementResult = await ExecuteSqlStatementAsync<TScalar>(statement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);

            //return result
            return statementResult.SingleResult;
        }

        /// <inheritdoc />
        public async Task<List<TResult?>> ExecuteSetAsync<TResult>(DataQuery<T> query)
        {
            return await ExecuteSetAsync<TResult>(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<TResult?>> ExecuteSetAsync<TResult>(DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate statement
            var statement = _sqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.CustomQuery, SqlStatementResultCardinality.MultipleRows, _map, query);

            // execute statement and map response
            var statementResult = await ExecuteSqlStatementAsync<TResult>(statement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);
            var result = statementResult.MultipleResults ?? new List<TResult?>();

            return result;
        }
    }
}