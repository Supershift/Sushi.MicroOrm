using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Defines methods to perform read only statement on database records and return them as objects.
    /// </summary>
    /// <typeparam name="T">Type to convert database recrods to</typeparam>
    public interface IReadOnlyConnector<T> 
    {
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> with a return value of <typeparamref name="TScalar"/>. 
        /// </summary>        
        Task<TScalar?> ExecuteScalarAsync<TScalar>(DataQuery<T> query);
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/> with a return value of <typeparamref name="TScalar"/>. 
        /// </summary>        
        Task<TScalar?> ExecuteScalarAsync<TScalar>(DataQuery<T> query, CancellationToken cancellationToken);
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="query"/>.
        /// </summary>        
        Task<List<TResult?>> ExecuteSetAsync<TResult>(DataQuery<T> query);
        /// <summary>
        /// Executes a custom SQL statement defined on <paramref name="query"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="query"/>.
        /// </summary>
        Task<List<TResult?>> ExecuteSetAsync<TResult>(DataQuery<T> query, CancellationToken cancellationToken);
        /// <summary>
        /// Gets all records from the database, using <paramref name="query"/> to build a where clause.
        /// </summary>
        Task<QueryListResult<T>> GetAllAsync(DataQuery<T> query);
        /// <summary>
        /// Gets all records from the database, using <paramref name="query"/> to build a where clause.
        /// </summary>
        Task<QueryListResult<T>> GetAllAsync(DataQuery<T> query, CancellationToken cancellationToken);
        /// <summary>
        /// Gets the first record from the resultset, using <paramref name="query"/> to build a where clause for <typeparamref name="T"/>.
        /// </summary>
        Task<T?> GetFirstAsync(DataQuery<T> query);
        /// <summary>
        /// Gets the first record from the resultset, using <paramref name="query"/> to build a where clause for <typeparamref name="T"/>.
        /// </summary>
        Task<T?> GetFirstAsync(DataQuery<T> query, CancellationToken cancellationToken);
        /// <summary>
        /// Creates a new instance of <see cref="DataQuery{T}"/>. 
        /// </summary>
        /// <returns></returns>
        DataQuery<T> CreateQuery();
    }
}
