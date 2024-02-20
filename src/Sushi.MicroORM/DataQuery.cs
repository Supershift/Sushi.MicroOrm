using Microsoft.Data.SqlClient;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Provides methods to build a SQL statement for use with <see cref="Connector{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataQuery<T> where T : new()
    {
        /// <summary>
        /// Creates an instance of <see cref="DataQuery{T}"/>.
        /// </summary>
        public DataQuery() : this(null) { }

        /// <summary>
        /// Creates an instance of <see cref="DataQuery{T}"/> using the specified mapping.
        /// </summary>
        public DataQuery(DataMap map)
        {
            //try to get the mapping declared for type T if no map provided
            if (map == null)
                map = DatabaseConfiguration.DataMapProvider.GetMapForType<T>();

            Map = map;
            if (Map == null)
                throw new Exception($"No default mapping defined for class {typeof(T)}. Apply a DataMap attribute to {typeof(T)} or create a datafilter with an instance of a mapping.");
        }

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
        public string OrderBy { get; private set; }

        /// <summary>
        /// Gets or sets a <see cref="PagingData"/> object that will be used to add paging to the SQL statement.
        /// </summary>
        public PagingData Paging { get; set; }

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
        public void Add(Expression<Func<T, object>> mappingExpression, object value)
        {
            Add(mappingExpression, value, ComparisonOperator.Equals);
        }

        /// <summary>
        /// Add a predicate to the WHERE clause using the column mapped to the property or field specified by <paramref name="mappingExpression"/>.
        /// </summary>
        /// <param name="mappingExpression"></param>
        /// <param name="value"></param>
        /// <param name="comparisonOperator"></param>
        public void Add(Expression<Func<T, object>> mappingExpression, object value, ComparisonOperator comparisonOperator)
        {
            var members = ReflectionHelper.GetMemberTree(mappingExpression);

            var dataproperty = Map.Items.FirstOrDefault(x => x.MemberInfoTree.SequenceEqual(members));
            if (dataproperty == null)
                throw new Exception($"Could not find member [{string.Join(".", members.Select(x => x.Name))}] for type {typeof(T)}");

            Add(dataproperty.Column, dataproperty.SqlType, value, comparisonOperator);
        }

        /// <summary>
        /// Adds a predicate to the WHERE clause using the specified column,
        /// using <see cref="ComparisonOperator.Equals"/> to compare to <paramref name="value"/>.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(string column, SqlDbType type, object value)
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
        public void Add(string column, SqlDbType type, object value, ComparisonOperator comparisonOperator)
        {
            var where = new WhereCondition(column, type, value, comparisonOperator);
            WhereClause.Add(where);
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
        public void AddOrder(Expression<Func<T, object>> memberExpression)
        {
            AddOrder(memberExpression, SortOrder.ASC);
        }

        /// <summary>
        /// Add a column to the ORDER BY clause that will be used to sort the result set.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="sortOrder"></param>
        public void AddOrder(Expression<Func<T, object>> memberExpression, SortOrder sortOrder)
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