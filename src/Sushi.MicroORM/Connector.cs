using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Sushi.MicroORM.Exceptions;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <inheritdoc />
    public class Connector<T> : ReadOnlyConnector<T>, IConnector<T>
    {
        private readonly ConnectionStringProvider _connectionStringProvider;
        private readonly SqlStatementGenerator _sqlStatementGenerator;        
        private readonly IExceptionHandler _exceptionHandler;
        private readonly MicroOrmOptions _options;

        /// <summary>
        /// An object representing the mapping between <typeparamref name="T"/> and database.
        /// </summary>
        private readonly DataMap<T> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class.
        /// </summary>
        public Connector(
            ConnectionStringProvider connectionStringProvider,
            IOptions<MicroOrmOptions> options,
            DataMapProvider dataMapProvider,
            SqlStatementGenerator sqlStatementGenerator,
            SqlExecuter sqlExecuter,
            IExceptionHandler exceptionHandler)
        : base(
            connectionStringProvider,
            options,
            dataMapProvider,
            sqlStatementGenerator,
            sqlExecuter
            )
        {
            _connectionStringProvider = connectionStringProvider;
            _options = options.Value;
            _sqlStatementGenerator = sqlStatementGenerator;            
            _exceptionHandler = exceptionHandler;
            _map = dataMapProvider.GetMapForType<T>();
        }
        
        /// <inheritdoc />
        public override DataQuery<T> CreateQuery()
        {
            return new DataQuery<T>(_map);
        }

        internal void AddPrimaryKeyToquery(DataQuery<T> query, T entity)
        {
            var primaryKeyColumns = _map.GetPrimaryKeyColumns();
            foreach (var column in primaryKeyColumns)
            {
                query.Add(column.Column, column.SqlType, ReflectionHelper.GetMemberValue(column.MemberInfoTree, entity));
            }

            if (primaryKeyColumns.Count == 0)
                throw new Exception("No primary key defined on mapping. Add at least on member mapped with Id().");
        }

        internal bool IsInsert(T entity)
        {
            var primaryKeyColumns = _map.GetPrimaryKeyColumns();
            var identityColumn = primaryKeyColumns.FirstOrDefault(x => x.IsIdentity);
            if (identityColumn == null)
                throw new Exception(@"No identity primary key column defined on mapping. Cannot determine if action is update or insert. 
Please map identity primary key column using Map.Id(). Otherwise use Insert or Update explicitly.");
            var currentIdentityValue = ReflectionHelper.GetMemberValue(identityColumn.MemberInfoTree, entity);
            return currentIdentityValue == null || currentIdentityValue as int? == 0;
        }

        /// <inheritdoc />
        public async Task SaveAsync(T entity)
        {
            await SaveAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task SaveAsync(T entity, CancellationToken cancellationToken)
        {
            if (IsInsert(entity))
                await InsertAsync(entity, false, cancellationToken).ConfigureAwait(false);
            else
                await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(T entity)
        {
            await UpdateAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
        {
            var query = new DataQuery<T>(_map);
            AddPrimaryKeyToquery(query, entity);
            await UpdateAsync(entity, query, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(T entity, DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate sql statement
            var sqlStatement = _sqlStatementGenerator.GenerateSqlStatment(DMLStatementType.Update, SqlStatementResultCardinality.None, _map, query, entity, false);

            // execute statement
            await ExecuteSqlStatementAsync<object>(sqlStatement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);
        }

        internal void ApplyIdentityColumnToEntity(T entity, int identityValue)
        {
            // validate input
            if(entity == null)
                throw new ArgumentNullException(nameof(entity));

            // use reflection to set the identity value on the entity's property mapped as identity column
            var identityColumn = _map.Items.FirstOrDefault(x => x.IsIdentity);
            if (identityValue > 0 && identityColumn != null)
                ReflectionHelper.SetMemberValue(identityColumn.MemberInfoTree, identityValue, entity, _options.DateTimeKind);
        }

        /// <inheritdoc />
        public async Task InsertOrUpdateAsync(T entity)
        {
            await InsertOrUpdateAsync(entity, false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task InsertOrUpdateAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken)
        {
            var query = new DataQuery<T>(_map);
            // generate query condition for primary key
            AddPrimaryKeyToquery(query, entity);

            // generate sql statement
            var statement = _sqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.InsertOrUpdate, SqlStatementResultCardinality.SingleRow, _map, query, entity, isIdentityInsert);

            // execute
            var response = await ExecuteSqlStatementAsync<int>(statement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);

            // if response contains a value map it to the idenity column
            if (response.SingleResult > 0)
            {
                ApplyIdentityColumnToEntity(entity, response.SingleResult);
            }
        }

        /// <inheritdoc />
        public async Task InsertAsync(T entity)
        {
            await InsertAsync(entity, false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task InsertAsync(T entity, bool isIdentityInsert)
        {
            await InsertAsync(entity, isIdentityInsert, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task InsertAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken)
        {
            // generate insert statement
            var query = CreateQuery();
            var sqlStatement = _sqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Insert, SqlStatementResultCardinality.SingleRow, _map, query, entity, isIdentityInsert);

            // execute and get response
            var response = await ExecuteSqlStatementAsync<int>(sqlStatement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);

            // if response contains a value map it to the idenity column
            if (response.SingleResult > 0)
            {
                ApplyIdentityColumnToEntity(entity, response.SingleResult);
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(T entity)
        {
            await DeleteAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
        {
            var query = new DataQuery<T>(_map);
            AddPrimaryKeyToquery(query, entity);
            await DeleteAsync(query, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(DataQuery<T> query)
        {
            await DeleteAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate delete statement
            var sqlStatement = _sqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Delete, SqlStatementResultCardinality.None, _map, query);

            // execute
            _ = await ExecuteSqlStatementAsync<object>(sqlStatement, query.ConnectionString, query.CommandTimeOut, query.IsReadOnly, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ExecuteNonQueryAsync(DataQuery<T> query)
        {
            await ExecuteNonQueryAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task ExecuteNonQueryAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            await ExecuteScalarAsync<int>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            await BulkInsertAsync(entities, false);
        }
        
        /// <inheritdoc />      
        public async Task BulkInsertAsync(IEnumerable<T> entities, bool identityInsert)
        {
            await BulkInsertAsync(entities, identityInsert, SqlBulkCopyOptions.Default, CancellationToken.None);
        }

        /// <inheritdoc />
        public async Task BulkInsertAsync(IEnumerable<T> entities, bool isIdentityInsert, SqlBulkCopyOptions sqlBulkCopyOptions, CancellationToken cancellationToken)
        {
            // todo: move all this logic away from connector to dedicated bulk insert objects, into meaningful testable methods
            if (entities?.Any() != true)
                return;

            var dataTable = Utility.CreateDataTableFromMap(_map, isIdentityInsert);

            //create rows in the datatable for each entity
            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();
                foreach (var databaseColumn in _map.Items.Where(x => x.IsReadOnly == false))
                {
                    //set values in the row for each column (and only if the column exists in the table definition)
                    if (dataTable.Columns.Contains(databaseColumn.Column))
                    {
                        var value = ReflectionHelper.GetMemberValue(databaseColumn.MemberInfoTree, entity);
                        //if null, we must use DBNull
                        if (value == null)
                        {
                            value = DBNull.Value;
                        }
                        row[databaseColumn.Column] = value;
                    }
                }
                dataTable.Rows.Add(row);
            }

            //create a sql connection (this allows sqlBulkCopy to enlist in a transaction scope, because the sqlConnection automatically enlists when open is called)
            string connectionString = _connectionStringProvider.GetConnectionString(typeof(T)).Primary;
            using var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            //insert using sqlBulkCopy
            using var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, null);
            
            //set command time out if a value was explicitly defined
            if (_options.DefaultCommandTimeOut.HasValue)
            {
                bulkCopy.BulkCopyTimeout = _options.DefaultCommandTimeOut.Value;
            }

            //we need to explicitly define a column mapping, otherwise the ordinal position of the columns in the datatable is used instead of name
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                var column = dataTable.Columns[i];
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            bulkCopy.DestinationTableName = dataTable.TableName;
            try
            {
                await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            }
            catch (TaskCanceledException) { throw; }
            catch (Exception ex)
            {
                throw _exceptionHandler.Handle(ex, null);
            }
        }
    }
}
