﻿using Microsoft.Data.SqlClient;
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
    /// <summary>
    /// Retrieves database records and returns them as objects, based on provided mapping.
    /// </summary>
    /// <typeparam name="T">Type to convert database recrods to</typeparam>
    public class Connector<T> where T : new()
    {
        private readonly ConnectionStringProvider _connectionStringProvider;
        private readonly SqlExecuter _sqlExecuter;

        /// <summary>
        /// An object representing the mapping between <typeparamref name="T"/> and database.
        /// </summary>
        private readonly DataMap<T> _map;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class.
        /// </summary>
        public Connector(
            ConnectionStringProvider connectionStringProvider,
            DataMapProvider dataMapProvider,
            SqlExecuter sqlExecuter)
        {
            _connectionStringProvider = connectionStringProvider;
            _sqlExecuter = sqlExecuter;
            _map = dataMapProvider.GetMapForType<T>() as DataMap<T>;
        }

        /// <summary>
        /// Gets or sets the wait time in seconds before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int? CommandTimeout { get; set; }

        private string _connectionString;
        

        /// <summary>
        /// Gets the connection string used to connect to the database. Setting this will override <see cref="ConnectionStringProvider"/>.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    _connectionString = _connectionStringProvider.GetConnectionString(typeof(T));
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="DataQuery{T}"/>. 
        /// </summary>
        /// <returns></returns>
        public DataQuery<T> CreateQuery()
        {
            return new DataQuery<T>(_map);
        }

        /// <summary>
        /// Executes the <paramref name="statement"/> and returns the result generated by the execution.
        /// </summary>        
        /// <returns></returns>
        private async Task<SqlStatementResult<TResult>> ExecuteSqlStatementAsync<TResult>(SqlStatement<T> statement, CancellationToken cancellationToken)
        {   
            var result = await _sqlExecuter.ExecuteAsync<T, TResult>(statement, ConnectionString, CommandTimeout, _map, cancellationToken);
            return result;
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

        /// <summary>
        /// Inserts or updates <paramref name="entity"/> in the database, based on primary key for <typeparamref name="T"/>. If the primary key is 0 or less, an insert is performed. Otherwise an update is performed.
        /// </summary>
        /// <param name="entity"></param>        
        public async Task SaveAsync(T entity)
        {
            await SaveAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts or updates <paramref name="entity"/> in the database, based on primary key for <typeparamref name="T"/>. If the primary key is 0 or less, an insert is performed. Otherwise an update is performed.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        public async Task SaveAsync(T entity, CancellationToken cancellationToken)
        {
            if (IsInsert(entity))
                await InsertAsync(entity, false, cancellationToken).ConfigureAwait(false);
            else
                await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates an instance of <see cref="DataQuery{T}" /> that can be used with <see cref="FetchSingle(int)"/> and <see cref="FetchSingleAsync(int)"/>.         
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected DataQuery<T> CreateFetchSinglequery(int id)
        {
            var primaryKeyColumns = _map.GetPrimaryKeyColumns();
            if (primaryKeyColumns.Count != 1)
                throw new Exception("Mapping does not have one and only one primary key column.");
            var primaryKeyColumn = primaryKeyColumns[0];            

            var query = new DataQuery<T>(_map);
            query.Add(primaryKeyColumn.Column, SqlDbType.Int, id);
            return query;
        } 

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="id"/> to build a where clause on <typeparamref name="T"/>'s primary key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(int id)
        {
            var query = CreateFetchSinglequery(id);

            return await FetchSingleAsync(query).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="query"/> to build a where clause for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(DataQuery<T> query)
        {
            return await FetchSingleAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sqlText"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            var statement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Select, SqlStatementResultCardinality.SingleRow, _map, query);            

            // execute and get response
            var statementResult = await ExecuteSqlStatementAsync<T>(statement, cancellationToken).ConfigureAwait(false);

            // return result
            return statementResult.SingleResult;
        }

        /// <summary>
        /// Updates the record <paramref name="entity"/> in the database.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync(T entity)
        {   
            await UpdateAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the record <paramref name="entity"/> in the database.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
        {
            List<WhereCondition> whereColumns = new List<WhereCondition>();
            var query = new DataQuery<T>(_map);            
            AddPrimaryKeyToquery(query, entity);
            await UpdateAsync(entity, query, cancellationToken).ConfigureAwait(false);
        }        

        /// <summary>
        /// Updates records in the database for <paramref name="query"/> using the values on <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        public async Task UpdateAsync(T entity, DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate sql statement
            var sqlStatement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Update, SqlStatementResultCardinality.None, _map, query, entity, false);

            // execute statement
            await ExecuteSqlStatementAsync<object>(sqlStatement, cancellationToken).ConfigureAwait(false);
        }

        internal void ApplyIdentityColumnToEntity(T entity, int identityValue)
        {
            var identityColumn = _map.Items.FirstOrDefault(x => x.IsIdentity);
            if (identityValue > 0 && identityColumn != null)
                ReflectionHelper.SetMemberValue(identityColumn.MemberInfoTree, identityValue, entity);
        }

        /// <summary>
        /// Inserts a new record for <typeparamref name="T"/> in the database if no record exists for the same primary key. Else the existing record is updated.
        /// </summary>        
        public async Task InsertOrUpdateAsync(T entity)
        {
            await InsertOrUpdateAsync(entity, false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts a new record for <typeparamref name="T"/> in the database if no record exists for the same primary key. Else the existing record is updated.
        /// </summary>        
        public async Task InsertOrUpdateAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken)
        {
            var query = new DataQuery<T>(_map);
            // generate query condition for primary key
            AddPrimaryKeyToquery(query, entity);

            // generate sql statement
            var statement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.InsertOrUpdate, SqlStatementResultCardinality.SingleRow, _map, query, entity, isIdentityInsert);

            // execute
            var response = await ExecuteSqlStatementAsync<int>(statement, cancellationToken).ConfigureAwait(false);

            // if response contains a value map it to the idenity column
            if (response.SingleResult > 0)
            {
                ApplyIdentityColumnToEntity(entity, response.SingleResult);
            }            
        }


        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>        
        /// <returns></returns>
        public async Task InsertAsync(T entity)
        {
            await InsertAsync(entity, false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed.</param>
        /// <returns></returns>
        public async Task InsertAsync(T entity, bool isIdentityInsert)
        {
            await InsertAsync(entity, isIdentityInsert, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InsertAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken)
        {
            // generate insert statement
            var query = CreateQuery();
            var sqlStatement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Insert, SqlStatementResultCardinality.SingleRow, _map, query, entity, isIdentityInsert);

            // execute and get response
            var response = await ExecuteSqlStatementAsync<int>(sqlStatement, cancellationToken).ConfigureAwait(false);

            // if response contains a value map it to the idenity column
            if (response.SingleResult > 0)
            {
                ApplyIdentityColumnToEntity(entity, response.SingleResult);
            }
        }

        /// <summary>
        /// Fetches all records from the database.
        /// </summary>        
        /// <returns></returns>
        public async Task<QueryListResult<T>> FetchAllAsync()
        {
            return await FetchAllAsync(CancellationToken.None).ConfigureAwait(false); 
        }

        /// <summary>
        /// Fetches all records from the database.
        /// </summary>        
        /// <returns></returns>
        public async Task<QueryListResult<T>> FetchAllAsync(CancellationToken cancellationToken)
        {
            var query = CreateQuery();
            return await FetchAllAsync(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database, using <paramref name="query"/> to build a where clause
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<QueryListResult<T>> FetchAllAsync(DataQuery<T> query)
        {
            return await FetchAllAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database, using <paramref name="query"/> to build a where clause.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<QueryListResult<T>> FetchAllAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate the sql statement            
            var statementType = DMLStatementType.Select;
            if (!string.IsNullOrWhiteSpace(query.SqlQuery))
                statementType = DMLStatementType.CustomQuery;
            var statement = SqlStatementGenerator.GenerateSqlStatment(statementType, SqlStatementResultCardinality.MultipleRows, _map, query);
            
            // execute and get response
            var statementResult = await ExecuteSqlStatementAsync<T>(statement, cancellationToken).ConfigureAwait(false);

            // if total number of rows is set apply it to the query's paging object
            if (query?.Paging != null && statementResult.TotalNumberOfRows.HasValue)
            {
                query.Paging.TotalNumberOfRows = statementResult.TotalNumberOfRows;
                statementResult.MultipleResults.TotalNumberOfRows = statementResult.TotalNumberOfRows;
                if (query.Paging.NumberOfRows > 0)
                {
                    statementResult.MultipleResults.TotalNumberOfPages = (int)Math.Ceiling((double)statementResult.MultipleResults.TotalNumberOfRows.Value / query.Paging.NumberOfRows);
                }
            }

            // return result
            return statementResult.MultipleResults;
        }

        /// <summary>
        /// Deletes <paramref name="entity"/> from the database
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(T entity)
        {            
            await DeleteAsync(entity, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes <paramref name="entity"/> from the database
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken)
        {
            var query = new DataQuery<T>(_map);
            AddPrimaryKeyToquery(query, entity);
            await DeleteAsync(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="query"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task DeleteAsync(DataQuery<T> query)
        {
            await DeleteAsync(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="query"/>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate delete statement
            var sqlStatement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.Delete, SqlStatementResultCardinality.None, _map, query);

            // execute
            _ = await ExecuteSqlStatementAsync<object>(sqlStatement, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> without a return value. Parameters can be defined on <paramref name="query"/>.
        /// </summary>        
        /// <param name="query"></param>
        public async Task ExecuteNonQueryAsync(DataQuery<T> query)
        {
            await ExecuteNonQueryAsync(query, CancellationToken.None).ConfigureAwait(false);            
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> without a return value. Parameters can be defined on <paramref name="query"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        public async Task ExecuteNonQueryAsync(DataQuery<T> query, CancellationToken cancellationToken)
        {
            await ExecuteScalarAsync<int>(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> with a return value of <typeparamref name="TScalar"/>. 
        /// </summary>        
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<TScalar> ExecuteScalarAsync<TScalar>(DataQuery<T> query)
        {
            return await ExecuteScalarAsync<TScalar>(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> with a return value of <typeparamref name="TScalar"/>. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TScalar> ExecuteScalarAsync<TScalar>(DataQuery<T> query, CancellationToken cancellationToken)
        {
            //generate the sql statement
            var statement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.CustomQuery, SqlStatementResultCardinality.SingleRow, _map, query);

            //execute and get response
            var statementResult = await ExecuteSqlStatementAsync<TScalar>(statement, cancellationToken).ConfigureAwait(false);

            //return result
            return statementResult.SingleResult;
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="query"/>.
        /// </summary>
        /// <param name="query"></param>        
        /// <returns></returns>
        public async Task<List<TResult>> ExecuteSetAsync<TResult>(DataQuery<T> query)
        {
            return await ExecuteSetAsync<TResult>(query, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="query"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<TResult>> ExecuteSetAsync<TResult>(DataQuery<T> query, CancellationToken cancellationToken)
        {
            // generate statement
            var statement = SqlStatementGenerator.GenerateSqlStatment<T>(DMLStatementType.CustomQuery, SqlStatementResultCardinality.MultipleRows, _map, query);

            // execute statement and map response
            var result = await ExecuteSqlStatementAsync<TResult>(statement, cancellationToken).ConfigureAwait(false);

            return result.MultipleResults;
        }

        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            await BulkInsertAsync(entities, false);
        }

        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        /// <param name="identityInsert"></param>        
        public async Task BulkInsertAsync(IEnumerable<T> entities, bool identityInsert)
        {
            await BulkInsertAsync(entities, identityInsert, SqlBulkCopyOptions.Default, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed. The default value is false.</param>
        /// <param name="sqlBulkCopyOptions"></param>
        public async Task BulkInsertAsync(IEnumerable<T> entities, bool isIdentityInsert, SqlBulkCopyOptions sqlBulkCopyOptions, CancellationToken cancellationToken)
        {
            if (entities == null || entities.Count() == 0)
                return;

            var dataTable = Utility.CreateDataTableFromMap(_map, isIdentityInsert);            

            //create rows in the datatable for each entity
            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();
                foreach (var databaseColumn in _map.Items.Where(x=>x.IsReadOnly == false))
                {
                    //set values in the row for each column (and only if the column exists in the table definition)
                    if (dataTable.Columns.Contains(databaseColumn.Column))
                    {
                        var value = ReflectionHelper.GetMemberValue(databaseColumn.MemberInfoTree, entity);
                        //if null, we must use DBNull
                        if (value == null)
                            value = DBNull.Value;
                        row[databaseColumn.Column] = value;
                    }
                }
                dataTable.Rows.Add(row);
            }

            //create a sql connection (this allows sqlBulkCopy to enlist in a transaction scope, because the sqlConnection automatically enlists when open is called)
            var start = DateTime.Now.Ticks;
            
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();
                //insert using sqlBulkCopy
                using (var bulkCopy = new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, null))
                {
                    //set command time out if a value was explicitly defined
                    if (this.CommandTimeout.HasValue)
                    {
                        bulkCopy.BulkCopyTimeout = this.CommandTimeout.Value;
                    }

                    //we need to explicitly define a column mapping, otherwise the ordinal position of the columns in the datatable is used instead of name
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var column = dataTable.Columns[i];
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    bulkCopy.DestinationTableName = dataTable.TableName;
                    await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                }
            }
        }                
    }
}
