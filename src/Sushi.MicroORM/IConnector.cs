using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Defines methods to interact with database records using objects, based on an object-relational mapping.
    /// </summary>
    /// <typeparam name="T">Type to convert database recrods to</typeparam>
    public interface IConnector<T> : IReadOnlyConnector<T>
    {
        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        Task BulkInsertAsync(IEnumerable<T> entities);
        
        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        /// <param name="identityInsert"></param>        
        Task BulkInsertAsync(IEnumerable<T> entities, bool identityInsert);

        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed. The default value is false.</param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="cancellationToken"></param>
        Task BulkInsertAsync(IEnumerable<T> entities, bool isIdentityInsert, SqlBulkCopyOptions sqlBulkCopyOptions, CancellationToken cancellationToken);
        
        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="query"/>
        /// </summary>        
        /// <returns></returns>
        Task DeleteAsync(DataQuery<T> query);
        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="query"/>
        /// </summary>        
        /// <returns></returns>
        Task DeleteAsync(DataQuery<T> query, CancellationToken cancellationToken);
        /// <summary>
        /// Deletes <paramref name="entity"/> from the database
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync(T entity);
        /// <summary>
        /// Deletes <paramref name="entity"/> from the database
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync(T entity, CancellationToken cancellationToken);
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> without a return value. Parameters can be defined on <paramref name="query"/>.
        /// </summary>        
        Task ExecuteNonQueryAsync(DataQuery<T> query);
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> without a return value. Parameters can be defined on <paramref name="query"/>.
        /// </summary>        
        Task ExecuteNonQueryAsync(DataQuery<T> query, CancellationToken cancellationToken);        
        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        Task InsertAsync(T entity);
        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed.</param>
        /// <returns></returns>
        Task InsertAsync(T entity, bool isIdentityInsert);
        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken);
        /// <summary>
        /// Inserts a new record for <typeparamref name="T"/> in the database if no record exists for the same primary key. Else the existing record is updated.
        /// </summary>        
        Task InsertOrUpdateAsync(T entity);
        /// <summary>
        /// Inserts a new record for <typeparamref name="T"/> in the database if no record exists for the same primary key. Else the existing record is updated.
        /// </summary>        
        Task InsertOrUpdateAsync(T entity, bool isIdentityInsert, CancellationToken cancellationToken);
        /// <summary>
        /// Inserts or updates <paramref name="entity"/> in the database, based on primary key for <typeparamref name="T"/>. If the primary key is 0 or less, an insert is performed. Otherwise an update is performed.
        /// </summary>
        Task SaveAsync(T entity);
        /// <summary>
        /// Inserts or updates <paramref name="entity"/> in the database, based on primary key for <typeparamref name="T"/>. If the primary key is 0 or less, an insert is performed. Otherwise an update is performed.
        /// </summary>
        Task SaveAsync(T entity, CancellationToken cancellationToken);
        /// <summary>
        /// Updates the record <paramref name="entity"/> in the database.
        /// </summary>
        Task UpdateAsync(T entity);
        /// <summary>
        /// Updates the record <paramref name="entity"/> in the database.
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken cancellationToken);
        /// <summary>
        /// Updates records in the database for <paramref name="query"/> using the values on <paramref name="entity"/>.
        /// </summary>
        Task UpdateAsync(T entity, DataQuery<T> query, CancellationToken cancellationToken);
    }
}