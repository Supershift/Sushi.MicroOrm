using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Provides methods to build a SQL statement for use with <see cref="Connector{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataQuery<T>
    {
        /// <summary>
        /// Creates an instance of <see cref="DataQuery{T}"/> using the specified mapping.
        /// </summary>
        public DataQuery(DataMap map)
        {   
            Map = map;
            if (Map == null)
            {
                throw new Exception($"No mapping defined for class {typeof(T)}. Add a nested class inhereting DataMap to {typeof(T)} or add a DataMap attribute to {typeof(T)}.");
            }
        }

        /// <summary>
        /// Gets or sets a custom SQL query to be executed. Setting this will make the <see cref="Connector{T}"/> ignore any other query input set, except parameters.
        /// </summary>
        public string? SqlQuery { get; set; }

        /// <summary>
        /// Gets an object representing the mapping between class T and database
        /// </summary>
        public DataMap Map { get; protected set; }        
        
        /// <summary>
        /// Gets or sets the maximum number of returned rows.
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Gets the ORDER BY clause that will be applied to the SQL statement to sort the result set. The column list can be appened using <see cref="AddOrder(Expression{Func{T, object}}, SortOrder)"/>.
        /// </summary>
        public string? OrderBy { get; private set; }
        
        /// <summary>
        /// Gets or sets a <see cref="PagingData"/> object that will be used to add paging to the SQL statement.
        /// </summary>
        public PagingData? Paging { get; set; }

        /// <summary>
        /// Gets or sets the connection string to use when executing this query. If left empty, the default connectionstring for <typeparamref name="T"/> will be used.
        /// </summary>
        public ConnectionString? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this query is a read-only statements. If true, execution can be optimized by using a read-only connection if available on the connection string.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Get or sets the command timeout to use when executing this query. If NULL, the default command timeout will be used.
        /// </summary>
        public int? CommandTimeOut { get; set; }

        internal List<SqlParameter> SqlParameters { get; } = new List<SqlParameter>();

        /// <summary>
        /// Adds a parameter and its value to the SQL statement. The SqlDbType for the parameter will be automatically determined. 
        /// Use this to specify parameters when using a custom SQL statement with <see cref="Connector{T}"/>.
        /// </summary>
        /// <param name="name"></param>        
        /// <param name="value"></param>        
        public void AddParameter<Y>(string name, Y value)
        {
            SqlParameter p = new SqlParameter();
            p.Value = value;
            p.ParameterName = name;
            p.SqlDbType = Utility.GetSqlDbType(typeof(Y));
            
            SqlParameters.Add(p);
        }

        /// <summary>
        /// Adds a parameter and its value to a SQL statement. 
        /// Use this to specify parameters when using a custom SQL statement with <see cref="Connector{T}"/>.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>        
        public void AddParameter(string parameterName, SqlDbType type, object value)
        {
            SqlParameter p = new SqlParameter();
            p.Value = value;
            p.ParameterName = parameterName;
            p.SqlDbType = type;
            
            SqlParameters.Add(p);
        }

        /// <summary>
        /// Adds a table valued parameter and its value to a SQL statement as parameter. The <paramref name="typeName"/> needs to be defined as user-defined type in SQL Server.        
        /// </summary>        
        public void AddParameter(string parameterName, DataTable tableValue, string typeName)
        {
            SqlParameter p = new SqlParameter();
            p.Value = tableValue;
            p.ParameterName = parameterName;
            p.SqlDbType = SqlDbType.Structured;            
            p.TypeName = typeName;
            
            SqlParameters.Add(p);
        }

        internal List<WhereCondition> WhereClause { get; } = new List<WhereCondition>();

        /// <summary>
        /// Add a predicate to the WHERE clause using the column mapped to the property or field specified by <paramref name="mappingExpression"/>, 
        /// using <see cref="ComparisonOperator.Equals"/> to compare to <paramref name="value"/>.
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="value"></param>        
        public void Add(Expression<Func<T, object?>> mappingExpression, object value)
        {
            Add(mappingExpression, value, ComparisonOperator.Equals);
        }

        /// <summary>
        /// Add a predicate to the WHERE clause using the column mapped to the property or field specified by <paramref name="mappingExpression"/>. 
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="value"></param>
        /// <param name="comparisonOperator"></param>
        public void Add(Expression<Func<T, object?>> mappingExpression, object value, ComparisonOperator comparisonOperator)
        {
            var members = ReflectionHelper.GetMemberTree(mappingExpression);

            var dataproperty = Map.Items.FirstOrDefault(x => x.MemberInfoTree.SequenceEqual(members));
            if (dataproperty == null)
                throw new Exception($"Could not find member [{string.Join(".", members.Select(x=>x.Name))}] for type {typeof(T)}");

            Add(dataproperty.Column, dataproperty.SqlType, value, comparisonOperator);            
        }

        /// <summary>
        /// Adds a predicate to the WHERE clause using the specified column,
        /// using <see cref="ComparisonOperator.Equals"/> to compare to <paramref name="value"/>.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>        
        public void Add(string column, SqlDbType type, object? value)
        {
            Add(column, type, value, ComparisonOperator.Equals);
        }

        /// <summary>
        /// Adds a predicate to the WHERE clause using the specified column. 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="comparisonOperator"></param>
        public void Add(string column, SqlDbType type, object? value, ComparisonOperator comparisonOperator)
        {
            var where = new WhereCondition(column, type, value, comparisonOperator);
            WhereClause.Add(where);
        }

        /// <summary>
        /// Shorthand method to add a predicate to the WHERE clause where <paramref name="value"/> is evaluated with a LIKE operator, e.g. WHERE column LIKE '%value%'.
        /// The '%' characters are automatically added to <paramref name="value"/>.
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="value"></param>
        public void AddLike(Expression<Func<T, object?>> mappingExpression, string value)
        {
            Add(mappingExpression, $"%{value}%", ComparisonOperator.Like);
        }

        /// <summary>
        /// Adds a plain text SQL search condition to the WHERE clause. 
        /// </summary>
        /// <param name="customSql"></param>
        public void AddSql(string customSql)
        {
            var where = new WhereCondition(customSql);
            WhereClause.Add(where);            
        }

        /// <summary>
        /// Add a column to the ORDER BY clause that will be used to sort the result set.
        /// </summary>
        /// <param name="memberExpression"></param>        
        public void AddOrder(Expression<Func<T, object?>> memberExpression)
        {
            AddOrder(memberExpression, SortOrder.ASC);
        }

        /// <summary>
        /// Add a column to the ORDER BY clause that will be used to sort the result set.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="sortOrder"></param>
        public void AddOrder(Expression<Func<T, object?>> memberExpression, SortOrder sortOrder)
        {
            var members = ReflectionHelper.GetMemberTree(memberExpression);

            var dataproperty = Map.Items.Where(x => x.MemberInfoTree.SequenceEqual(members)).FirstOrDefault();
            if (dataproperty == null)
                throw new Exception($"Could not find member [{string.Join(".", members.Select(x => x.Name))}] for type {typeof(T)}");

            AddOrder(dataproperty.Column, sortOrder);
        }

        /// <summary>
        /// Add a column to the ORDER BY clause that will be used to sort the result set.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="sortOrder"></param>
        public void AddOrder(string column, SortOrder sortOrder)
        {
            string sortOrderValue = "ASC";
            if (sortOrder == SortOrder.DESC)
                sortOrderValue = "DESC";

            if (string.IsNullOrWhiteSpace(OrderBy))
                OrderBy = $" ORDER BY {column} {sortOrderValue}";
            else
                OrderBy += $", {column} {sortOrderValue}";
        }

        /// <summary>
        /// Adds paging using OFFSET, ROWS FETCH NEXT.
        /// </summary>
        /// <param name="numberOfRows"></param>
        /// <param name="pageIndex"></param>
        public void AddPaging(int numberOfRows, int pageIndex)
        {
            Paging = new PagingData(numberOfRows, pageIndex);
        }
    }
}
