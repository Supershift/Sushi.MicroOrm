using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class.
        /// </summary>
        public Connector() : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class, using <paramref name="connectionString"/> instead of the default connection string for <typeparamref name="T"/>.
        /// </summary>
        public Connector(string connectionString) : this(connectionString, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class, using <paramref name="map"/> instead of the default map for <typeparamref name="T"/>.
        /// </summary>
        public Connector(DataMap<T> map) : this(null, map) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector{T}"/> class, using <paramref name="connectionString"/> and <paramref name="map"/> instead of the default connection string and map for <typeparamref name="T"/>.
        /// </summary>
        public Connector(string connectionString, DataMap<T> map)
        {
            if(map == null)                
                map = DatabaseConfiguration.DataMapProvider.GetMapForType<T>() as DataMap<T>;

            Map = map;
            if (Map == null)
                throw new Exception($"No mapping defined for class {typeof(T)}");

            if (!string.IsNullOrWhiteSpace(connectionString))
                ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the wait time in seconds before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int? CommandTimeout { get; set; }

        private string _ConnectionString;
        /// <summary>
        /// Gets the connection string used to connect to the database
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ConnectionString))
                    _ConnectionString = DatabaseConfiguration.ConnectionStringProvider.GetConnectionString(typeof(T));
                return _ConnectionString;
            }
            protected set
            {
                _ConnectionString = value;
            }
        }

        /// <summary>
        /// Gets an object representing the mapping between <typeparamref name="T"/> and database.
        /// </summary>
        public DataMap Map { get; protected set; }

        /// <summary>
        /// Creates a new instance of <see cref="DataFilter{T}"/>. Use the constructor of DataFilter for more control when creating a DataFilter.
        /// </summary>
        /// <returns></returns>
        public DataFilter<T> CreateDataFilter()
        {
            return new DataFilter<T>(Map);
        }        

        internal void AddPrimaryKeyToFilter(DataFilter<T> filter, T entity)
        {
            var primaryKeyColumns = Map.GetPrimaryKeyColumns();
            foreach (var column in primaryKeyColumns)
            {
                filter.Add(column.Column, column.SqlType, column.Info.GetValue(entity));
            }
        }

        internal bool IsInsert(T entity)
        {
            var primaryKeyColumns = Map.GetPrimaryKeyColumns();
            var identityColumn = primaryKeyColumns.FirstOrDefault(x => x.IsIdentity);
            if (identityColumn == null)
                throw new Exception(@"No identity primary key column defined on mapping. Cannot determine if action is update or insert. 
Please map identity primary key column using Map.Id(). Otherwise use Insert or Update explicitly.");
            var currentIdentityValue = identityColumn.Info.GetValue(entity);
            return currentIdentityValue == null || currentIdentityValue as int? == 0;
        }

        /// <summary>
        /// Inserts or updates <paramref name="entity"/> in the database, based on primary key for <typeparamref name="T"/>. If the primary key is 0 or less, an insert is performed. Otherwise an update is performed.
        /// </summary>
        /// <param name="entity"></param>
        public void Save(T entity)
        {            
            if (IsInsert(entity))
                Insert(entity);
            else
                Update(entity);

            Map.OnAfterSave(Map);
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

            Map.OnAfterSave(Map);
        }

        /// <summary>
        /// Creates an instance of <see cref="DataFilter{T}" /> that can be used with <see cref="FetchSingle(int)"/> and <see cref="FetchSingleAsync(int)"/>. 
        /// Throws an exception if mapping for <see cref="Map"/> does not have one and only one primary key column and which is mapped to <see cref="int"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected DataFilter<T> GetFetchSingleFilter(int id)
        {
            var primaryKeyColumns = Map.GetPrimaryKeyColumns();
            if (primaryKeyColumns.Count != 1)
                throw new Exception("Mapping does not have one and only one primary key column.");
            var primaryKeyColumn = primaryKeyColumns[0];

            var primaryKeyType = primaryKeyColumn.Info.PropertyType;
            if (primaryKeyType != typeof(int))
                throw new Exception("Primary key column is not mapped to a property of type 'Int32'.");

            var filter = new DataFilter<T>(Map);
            filter.Add(primaryKeyColumn.Column, SqlDbType.Int, id);
            return filter;
        }

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="id"/> to build a where clause on <typeparamref name="T"/>'s primary key.
        /// Only works if the mapping for <typeparamref name="T"/> has one and only one primary key column and it is mapped to <see cref="int"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T FetchSingle(int id)
        {
            var filter = GetFetchSingleFilter(id);

            return FetchSingle(filter);
        }        

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. 
        /// </summary>        
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public T FetchSingle(string sqlText)
        {
            return FetchSingle(sqlText, null);
        }

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="filter"/> to build a where clause for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public T FetchSingle(DataFilter<T> filter)
        {
            return FetchSingle(null, filter);
        }

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. Parameters used in <paramref name="sqlText"/> can be set on <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public T FetchSingle(string sqlText, DataFilter<T> filter)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                var query = ApplyFilterToCommanderFetchSingle(dac, sqlText, filter);

                Map.OnBeforeFetch(Map, query);
                if (query.Result != null)
                    return (T)query.Result;
                
                SqlDataReader reader = dac.ExecReader();
                var result = CreateFetchSingleResultFromReader(query, reader);
                query.Result = result;
                Map.OnAfterFetch(Map, query);
                return result;
            }
        }

        internal Query ApplyFilterToCommanderFetchSingle(SqlCommander dac, string sql, DataFilter<T> filter)
        {
            Query query = new Query();
            query.From = Map.TableName;
            query.OrderBy = filter?.OrderBy;

            string parameterinfo = null;
            query.Where = CreateWhereClause(filter, dac, out parameterinfo);
            query.ParameterInfo = parameterinfo;

            ApplySelect(Map, query);

            if (query.Select.Count == 0)
                throw new Exception("No columns set for the select statement");

            if (string.IsNullOrWhiteSpace(sql))
            {
                Map.OnSelectQueryCreation(Map, query);
                query.Sql = $"SELECT TOP(1) {(string.Join(", ", query.Select.Select(x => x.ColumnAlias).ToArray()))} FROM {query.From} {query.Where} {query.OrderBy}";
            }
            else
                query.Sql = sql;

            dac.SqlText = query.Sql;
            return query;
        }

        internal void ApplySelect(DataMap map, Query query)
        {
            foreach (var param in map.DatabaseColumns)
            {
                if (!string.IsNullOrWhiteSpace(param.Column))
                    query.Select.Add(param);
            }
        }

        private void SetResultValuesToObject(Query query, SqlDataReader reader, object instance)
        {
            foreach (var param in query.Select)
            {
                for (int column = 0; column < reader.FieldCount; column++)
                {
                    //get the name and the value of the column
                    var columnName = reader.GetName(column);
                    if (param.ColumnName.Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var value = reader.GetValue(column);
                        try
                        {
                            if (param.HasReflection)
                                param.OnReflection(instance, param, value);
                            else
                                ReflectionHelper.SetPropertyValue(param.Info, value, instance);
                        }
                        catch(Exception)
                        {
                            Console.WriteLine($"Could not match {columnName}");
                        }
                    }
                }
            }
        }

        internal T CreateFetchSingleResultFromReader(Query query, SqlDataReader reader)
        {
            T instance = new T();
            bool recordFound = reader.Read();
            if (recordFound)
            {
                SetResultValuesToObject(query, reader, instance);                
            }

            if (!recordFound)
            {
                switch (DatabaseConfiguration.FetchSingleMode)
                {
                    case FetchSingleMode.ReturnDefaultWhenNotFound:
                        instance = default(T);
                        break;
                    case FetchSingleMode.ReturnNewObjectWhenNotFound:
                        break;
                }
            }

            return instance;
        }

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="id"/> to build a where clause on <typeparamref name="T"/>'s primary key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(int id)
        {
            var filter = GetFetchSingleFilter(id);

            return await FetchSingleAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using <paramref name="filter"/> to build a where clause for <typeparamref name="T"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(DataFilter<T> filter)
        {
            return await FetchSingleAsync(null, filter).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. 
        /// </summary>        
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(string sqlText)
        {
            return await FetchSingleAsync(sqlText, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. Parameters used in <paramref name="sqlText"/> can be set on <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(string sqlText, DataFilter<T> filter)
        {
            return await FetchSingleAsync(sqlText, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a single record from the database, using the query provided by <paramref name="sqlText"/>. Parameters used in <paramref name="sqlText"/> can be set on <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sqlText"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> FetchSingleAsync(string sqlText, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                var query = ApplyFilterToCommanderFetchSingle(dac, sqlText, filter);
                Map.OnBeforeFetch(Map, query);
                if (query.Result != null)
                    return (T)query.Result;

                SqlDataReader reader = await dac.ExecReaderAsync(cancellationToken).ConfigureAwait(false);
                var result = CreateFetchSingleResultFromReader(query, reader);

                query.Result = result;
                Map.OnAfterFetch(Map, query);

                return result;
            }
        }

        string CreateWhereClause(DataFilter<T> filter, SqlCommander dac)
        {
            var parameterinfo = string.Empty;
            return CreateWhereClause(filter, dac, out parameterinfo);
        }

        //todo: move to where clause generator
        private string CreateWhereClause(DataFilter<T> filter, SqlCommander dac, out string parameterinfo)
        {
            parameterinfo = string.Empty;
            
            var whereClause = filter?.WhereClause;
            
            if (whereClause != null)
            {
                var count = (from item in whereClause where item == null select item);
                if (count.Count() > 0)
                    whereClause = (from item in whereClause where item != null select item).ToList();
            }            

            if (filter?.SqlParameters != null)
            {
                foreach (SqlParameter p in filter.SqlParameters)
                {
                    parameterinfo += $"{p.ParameterName}={p.Value} ";
                    dac.SetParameterInput(p.ParameterName, p.Value, p.SqlDbType, p.TypeName);
                }
            }

            if (whereClause == null || whereClause.Count == 0) return null;
            string sqlWhereClause = "where ";

            int index = 0;

            bool orGroupIsSet = false;

            foreach (WhereCondition predicate in whereClause)
            {
                string param = string.Concat("@C", index);                
                                
                WhereCondition nextcolumn = null;

                while (nextcolumn == null)
                {
                    if (whereClause.Count > index + 1)
                    {
                        nextcolumn = whereClause[index + 1];

                        if (predicate.ConnectType == WhereConditionOperator.And && nextcolumn.ConnectType == WhereConditionOperator.Or)
                        {
                            orGroupIsSet = true;
                            sqlWhereClause += "(";
                        }
                    }
                    else
                        break;
                }

                //if custom sql was provided for this predicate, add that to the where clause. otherwise, build the sql for the where clause
                if (!string.IsNullOrWhiteSpace(predicate.SqlText))
                {
                    sqlWhereClause += predicate.SqlText;
                }
                else
                {
                    switch (predicate.CompareType)
                    {
                        case ComparisonOperator.Equals:
                            //if we need to compare to a NULL value, use an 'IS NULL' predicate
                            if (predicate.Value == null)
                            {
                                sqlWhereClause += string.Concat(predicate.Column, " IS NULL");
                            }
                            else
                            {
                                sqlWhereClause += string.Concat(predicate.Column, " = ", param);
                                if (dac != null)
                                {
                                    dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                    parameterinfo += $"{param}={predicate.Value} ";
                                }
                            }
                            break;
                        case ComparisonOperator.NotEqualTo:
                            //if we need to compare to a NULL value, use an 'IS NOT NULL' predicate
                            if (predicate.Value == null)
                            {
                                sqlWhereClause += string.Concat(predicate.Column, " IS NOT NULL");
                            }
                            else
                            {
                                sqlWhereClause += string.Concat(predicate.Column, " <> ", param);
                                if (dac != null)
                                {
                                    dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                    parameterinfo += $"{param}={predicate.Value} ";
                                }
                            }
                            break;
                        case ComparisonOperator.Like:
                            sqlWhereClause += string.Concat(predicate.Column, " LIKE ", param);
                            if (dac != null)
                            {
                                dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                parameterinfo += $"{param}={predicate.Value} ";
                            }
                            break;
                        case ComparisonOperator.In:
                            if (predicate.Value is IEnumerable)
                            {
                                var items = (IEnumerable)predicate.Value;

                                //create a unique parameter for each item in the 'IN' predicate
                                var inParams = new List<string>();
                                int i = 0;
                                if (items != null)
                                {
                                    foreach (var item in items)
                                    {
                                        string inParam = $"{param}_{i}";

                                        dac.SetParameterInput(inParam, item, predicate.SqlType, predicate.Length);
                                        inParams.Add(inParam);

                                        i++;
                                    }
                                }
                                //if there are items in the collection, add a predicate to the where in clause. 
                                //if not, add a predicate that always evaluates to false, because no row will match the empty values
                                if (inParams.Count > 0)
                                    sqlWhereClause += $"{predicate.Column} IN ({string.Join(",", inParams)})";
                                else
                                    sqlWhereClause += "1 = 0";
                            }
                            else
                            {
                                throw new Exception($"Cannot build WHERE clause. When using {nameof(ComparisonOperator.In)}, supply an IEnumerable as value.");
                            }
                            break;
                        case ComparisonOperator.GreaterThan:
                            sqlWhereClause += string.Concat(predicate.Column, " > ", param);
                            if (dac != null)
                            {
                                dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                parameterinfo += $"{param}={predicate.Value} ";
                            }
                            break;
                        case ComparisonOperator.GreaterThanOrEquals:
                            sqlWhereClause += string.Concat(predicate.Column, " >= ", param);
                            if (dac != null)
                            {
                                dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                parameterinfo += $"{param}={predicate.Value} ";
                            }
                            break;
                        case ComparisonOperator.LessThan:
                            sqlWhereClause += string.Concat(predicate.Column, " < ", param);
                            if (dac != null)
                            {
                                dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                parameterinfo += $"{param}={predicate.Value} ";
                            }
                            break;
                        case ComparisonOperator.LessThanOrEquals:
                            sqlWhereClause += string.Concat(predicate.Column, " <= ", param);
                            if (dac != null)
                            {
                                dac.SetParameterInput(param, predicate.Value, predicate.SqlType, predicate.Length);
                                parameterinfo += $"{param}={predicate.Value} ";
                            }
                            break;
                    }
                }
                
                if (nextcolumn != null)
                {
                    if (nextcolumn.ConnectType == WhereConditionOperator.And)
                    {
                        if (orGroupIsSet)
                        {
                            orGroupIsSet = false;
                            sqlWhereClause += ") and ";
                        }
                        else
                            sqlWhereClause += " and ";
                    }
                    else if (nextcolumn.ConnectType == WhereConditionOperator.Or || nextcolumn.ConnectType == WhereConditionOperator.OrUngrouped)
                    {
                        sqlWhereClause += " or ";
                    }
                }
                index++;
            }

            if (orGroupIsSet) sqlWhereClause += ")";
                        
            return sqlWhereClause;
        }
                
        /// <summary>
        /// Updates the record <paramref name="entity"/> in the database.
        /// </summary>
        /// <returns></returns>
        public void Update(T entity)
        {
            List<WhereCondition> whereColumns = new List<WhereCondition>();
            var filter = new DataFilter<T>(Map);            
            AddPrimaryKeyToFilter(filter, entity);
            Update(entity, filter);
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
            var filter = new DataFilter<T>(Map);            
            AddPrimaryKeyToFilter(filter, entity);
            await UpdateAsync(entity, filter, cancellationToken).ConfigureAwait(false);
        }

        internal string ApplyUpsertToSqlCommander(SqlCommander dac, T entity, DataFilter<T> filter, bool isIdentityInsert = false)
        {
            string updateColumns = " ";
            string whereClause = "";
            string insertColumns = " ";
            string valuesColumns = "";
            string primaryParameter = null;
            string returnCall = null;
            DataMapItem primaryDataColumn = null;

            foreach (var param in Map.DatabaseColumns)
            {
                if (param.IsReadOnly) continue;

                //  Insert
                if (!isIdentityInsert && param.IsIdentity && param.Column != null)
                {
                    primaryDataColumn = param;
                    returnCall = string.Format("SET @{0} = SCOPE_IDENTITY()", param.Column);
                    primaryParameter = "@" + param.Column;
                    dac.SetParameterOutput(primaryParameter, param.SqlType, param.Length);
                }
                else
                {
                    if (insertColumns.Contains(string.Concat(" ", param.Column, ", ")))
                        continue;

                    insertColumns += string.Concat(param.Column, ", ");
                    valuesColumns += string.Concat("@", param.Column, ", ");
                }

                //  Update
                if (param.IsPrimaryKey) continue;
                //  Double check
                if (updateColumns.Contains(string.Concat(" ", param.Column, "= ")))
                    continue;

                updateColumns += string.Concat(param.Column, "= ", "@", param.Column, ", ");
                dac.SetParameterInput(string.Concat("@", param.Column), param.Info.GetValue(entity), param.SqlType, param.Length);
            }

            if (filter?.SqlParameters != null)
            {
                foreach (SqlParameter p in filter.SqlParameters)
                {
                    updateColumns += string.Concat(p.ParameterName, "= ", "@", p.ParameterName, ", ");
                    dac.SetParameterInput(string.Concat("@", p.ParameterName), p.Value, p.SqlDbType);
                }
            }

            if (updateColumns.Length == 0) 
                return null;

            whereClause = CreateWhereClause(filter, dac);

            string sqlText = string.Format(@"IF EXISTS(select * from {0} {5}) BEGIN update {0} set {1} {5} END ELSE BEGIN insert into {0} ({2}) values ({3}) {4} END",
                Map.TableName,
                updateColumns.Substring(0, updateColumns.Length - 2),
                insertColumns.Substring(0, insertColumns.Length - 2),
                valuesColumns.Substring(0, valuesColumns.Length - 2),
                returnCall,
                whereClause);

            dac.SqlText = sqlText;
            return primaryParameter;
        }

        internal void ApplyUpdateToSqlCommander(SqlCommander dac, T entity, DataFilter<T> filter)
        {
            string updateColumns = " ";
            string whereClause = "";

            foreach (var param in Map.DatabaseColumns)
            {
                if (param.IsPrimaryKey) continue;
                if (param.IsReadOnly) continue;
                //  Double check
                if (updateColumns.Contains(string.Concat(" ", param.Column, "= ")))
                    continue;

                updateColumns += string.Concat(param.Column, "= ", "@", param.Column, ", ");
                dac.SetParameterInput(string.Concat("@", param.Column), param.Info.GetValue(entity), param.SqlType, param.Length);

            }

            if (filter?.SqlParameters != null)
            {
                foreach (SqlParameter p in filter.SqlParameters)
                {
                    updateColumns += string.Concat(p.ParameterName, "= ", "@", p.ParameterName, ", ");
                    dac.SetParameterInput(string.Concat("@", p.ParameterName), p.Value, p.SqlDbType);
                }
            }

            if (updateColumns.Length == 0) return;

            whereClause = CreateWhereClause(filter, dac);

            string sqlText = string.Format("update {0} set {1} {2}",
                Map.TableName,
                updateColumns.Substring(0, updateColumns.Length - 2),
                whereClause);


            dac.SqlText = sqlText;
        }

        /// <summary>
        /// Updates records in the database for <paramref name="filter"/> using the values on <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        public void Update(T entity, DataFilter<T> filter)
        {            
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                ApplyUpdateToSqlCommander(dac, entity, filter);
                dac.ExecNonQuery();                
            }
        }


        /// <summary>
        /// Updates records in the database for <paramref name="filter"/> using the values on <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        public async Task UpdateAsync(T entity, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                ApplyUpdateToSqlCommander(dac, entity, filter);
                await dac.ExecNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Applies all settings to an instance of <see cref="SqlCommander"/> to perform an insert for <typeparamref name="T"/>.
        /// If <typeparamref name="T"/> has an IdentityColumn, the output parameter name with the newly inserted identity is returned.
        /// </summary>
        /// <param name="dac"></param>
        /// <param name="entity"></param>
        /// <param name="identityInsert"></param>
        /// <returns></returns>
        internal string ApplyInsertToSqlCommander(SqlCommander dac, T entity, bool identityInsert)
        {
            string insertColumns = " ";
            string valuesColumns = "";
            string primaryParameter = null;
            string returnCall = null;
            DataMapItem primaryDataColumn = null;

            foreach (var databaseColumn in Map.DatabaseColumns)
            {
                if (!identityInsert && databaseColumn.IsIdentity && databaseColumn.Column != null)
                {
                    primaryDataColumn = databaseColumn;
                    returnCall = string.Format("SET @{0} = SCOPE_IDENTITY()", databaseColumn.Column);
                    primaryParameter = "@" + databaseColumn.Column;
                    dac.SetParameterOutput(primaryParameter, databaseColumn.SqlType, databaseColumn.Length);
                }
                else
                {
                    if (databaseColumn.IsReadOnly) continue;

                    //  Double check
                    if (insertColumns.Contains(string.Concat(" ", databaseColumn.Column, ", ")))
                        continue;

                    insertColumns += string.Concat(databaseColumn.Column, ", ");
                    valuesColumns += string.Concat("@", databaseColumn.Column, ", ");
                    dac.SetParameterInput(string.Concat("@", databaseColumn.Column), databaseColumn.Info.GetValue(entity), databaseColumn.SqlType, databaseColumn.Length);
                }
            }

            string sqlText;

            if (insertColumns.Length == 0)
            {
                sqlText = string.Format("insert into {0} DEFAULT VALUES", Map.TableName);
            }
            else
            {
                sqlText = string.Format("insert into {0} ({1}) values ({2}) {3}",
                    Map.TableName,
                    insertColumns.Substring(0, insertColumns.Length - 2),
                    valuesColumns.Substring(0, valuesColumns.Length - 2),
                    returnCall);
            }

            dac.SqlText = sqlText;

            return primaryParameter;
        }

        internal void ApplyIdentityColumnToEntity(T entity, SqlCommander dac, string identityOutputParameter)
        {
            if (!string.IsNullOrWhiteSpace(identityOutputParameter))
            {
                var identityColumn = Map.DatabaseColumns.FirstOrDefault(x => x.IsIdentity);
                var id = dac.GetParamInt(identityOutputParameter);
                if (id > 0)
                    identityColumn?.Info.SetValue(entity, id, null);
            }
        }

        /// <summary>
        /// Inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isIdentityInsert">When false, the primary key is set by the database. If true, an identity insert is performed</param>
        /// <returns></returns>
        public void Insert(T entity, bool isIdentityInsert = false)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                string identityParameter = ApplyInsertToSqlCommander(dac, entity, isIdentityInsert);

                dac.ExecNonQuery();

                ApplyIdentityColumnToEntity(entity, dac, identityParameter);                
            }
        }

        /// <summary>
        /// Updates or inserts <typeparamref name="T"/> in the database.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        /// <param name="isIdentityInsert"></param>
        public void Upsert(T entity, DataFilter<T> filter, bool isIdentityInsert = false)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                string identityParameter = ApplyUpsertToSqlCommander(dac, entity, filter, isIdentityInsert);
                dac.ExecNonQuery();
                
                if (!string.IsNullOrWhiteSpace(identityParameter))
                    ApplyIdentityColumnToEntity(entity, dac, identityParameter);
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
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                string identityParameter = ApplyInsertToSqlCommander(dac, entity, isIdentityInsert);

                await dac.ExecNonQueryAsync(cancellationToken).ConfigureAwait(false);

                ApplyIdentityColumnToEntity(entity, dac, identityParameter);
            }
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>.
        /// </summary>
        /// <param name="sqlText"></param>        
        /// <returns></returns>
        public List<T> FetchAll(string sqlText)
        {
            return FetchAll(sqlText, null);
        }

        /// <summary>
        /// Fetches all records from the database, using <paramref name="filter"/> to build a where clause
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<T> FetchAll(DataFilter<T> filter)
        {
            return FetchAll(null, filter);
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>, using parameters set on <paramref name="filter"/>
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<T> FetchAll(string sqlText, DataFilter<T> filter)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                var query = ApplyFilterToCommanderFetchAll(dac, filter, sqlText);

                Map.OnBeforeFetch(Map, query);
                if (query.Result != null)
                    return (List<T>)query.Result;

                using (SqlDataReader reader = dac.ExecReader())
                {
                    var result = new List<T>();
                    var r = new Dictionary<object, object>();

                    //read the first result set
                    while (reader.Read())
                    {
                        T entity = new T();
                        SetResultValuesToObject(query, reader, entity);
                        
                        result.Add(entity);
                    }

                    //if we have a second result set, it is the filter's paging
                    if (reader.NextResult() && filter != null && filter.Paging != null)
                    {
                        if (reader.Read())
                        {
                            var candidate = reader.GetValue(0);
                            if (candidate != null && candidate is int)
                                filter.Paging.TotalNumberOfRows = (int)candidate;
                        }
                    }

                    query.Result = result;
                    Map.OnAfterFetch(Map, query);

                    return result;
                }
            }
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>.
        /// </summary>        
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(string sqlText)
        {
            return await FetchAllAsync(sqlText, null, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>.
        /// </summary>        
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(string sqlText, CancellationToken cancellationToken)
        {
            return await FetchAllAsync(sqlText, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database, using <paramref name="filter"/> to build a where clause
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(DataFilter<T> filter)
        {
            return await FetchAllAsync(null, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database, using <paramref name="filter"/> to build a where clause
        /// </summary>        
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(DataFilter<T> filter, CancellationToken cancellationToken)
        {
            return await FetchAllAsync(null, filter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>, using parameters set on <paramref name="filter"/>
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(string sqlText, DataFilter<T> filter)
        {
            return await FetchAllAsync(sqlText, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all records from the database for <paramref name="sqlText"/>, using parameters set on <paramref name="filter"/>
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<T>> FetchAllAsync(string sqlText, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                var query = ApplyFilterToCommanderFetchAll(dac, filter, sqlText);
                Map.OnBeforeFetch(Map, query);
                if (query.Result != null)
                    return (List<T>)query.Result;

                using (SqlDataReader reader = await dac.ExecReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var result = new List<T>();

                    //read the first result set
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        T entity = new T();
                        SetResultValuesToObject(query, reader, entity);
                        result.Add(entity);
                    }

                    //if we have a second result set, it is the filter's paging
                    if (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false) && filter != null && filter.Paging != null)
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var candidate = reader.GetValue(0);
                            if (candidate != null && candidate is int)
                                filter.Paging.TotalNumberOfRows = (int)candidate;
                        }
                    }

                    query.Result = result;
                    Map.OnAfterFetch(Map, query);

                    return result;
                }
            }
        }

        internal Query ApplyFilterToCommanderFetchAll(SqlCommander dac, DataFilter<T> filter, string customSql)
        {
            Query query = new Query();
            query.From = Map.TableName;
            query.OrderBy = filter?.OrderBy;
            query.Where = CreateWhereClause(filter, dac);

            ApplySelect(Map, query);

            if (query.Select.Count == 0) return query;

            string rowcount = null;
            if (filter?.MaxResults != null) rowcount = $"TOP({filter.MaxResults.Value})";

            //apply paging if supplied on data request
            //a count query is run first, then a query to retrieve the data from the page 
            //todo: make both queries run in one roundtrip
            string pagingOffset = null;
            string pagingCountQuery = null;
            if (filter?.Paging != null && filter?.Paging?.NumberOfRows > 0)
            {                
                //create count query
                pagingCountQuery = $"\n\rSELECT COUNT(*) FROM {query.From} {query.Where}";

                //create offset query text 
                //TODO: use parameters for this
                pagingOffset = $"OFFSET {filter.Paging.PageIndex * filter.Paging.NumberOfRows} ROWS FETCH NEXT {filter.Paging.NumberOfRows} ROWS ONLY";

                //if offset is used, it always needs an order by clause. create one if none supplied
                if (string.IsNullOrWhiteSpace(filter.OrderBy))
                {
                    var primaryKeyColumns = Map.GetPrimaryKeyColumns();
                    if (primaryKeyColumns.Count > 0)
                        query.OrderBy = "ORDER BY " + string.Join(",", primaryKeyColumns.Select(x => x.Column));
                }
                //if paging is applied, the offset/fetch next method is used. no need to apply TOP() in this case                    
                rowcount = null;
            }

            //create the select statement, except when the caller supplied his own sql statement
            string sqlText;
            if (string.IsNullOrWhiteSpace(customSql))
            {
                Map.ValidateMappingForGeneratedQueries();
                Map.OnSelectQueryCreation(Map, query);
                query.Sql = $"SELECT {rowcount} {(string.Join(", ", query.Select.Select(x => x.ColumnAlias).ToArray()))} FROM {query.From} {query.Where} {query.OrderBy} {pagingOffset}";
            }
            else
                query.Sql = customSql;

            //append paging's count query if we have one
            if(pagingCountQuery != null)
                query.Sql += pagingCountQuery;

            dac.SqlText = query.Sql;
            return query;
        }

        internal void ApplyDeleteToSqlCommander(SqlCommander dac, DataFilter<T> filter)
        {
            string whereClause = "";
            whereClause = CreateWhereClause(filter, dac);
            if (whereClause.Length == 0) return;

            string sqlText = string.Format("delete from {0} {1}",
                Map.TableName,
                whereClause);

            dac.SqlText = sqlText;            
        }

        /// <summary>
        /// Deletes <paramref name="entity"/> from the database
        /// </summary>
        /// <returns></returns>
        public void Delete(T entity)
        {
            var filter = new DataFilter<T>(Map);
            //filter.Add(PrimaryKeyColumn, SqlDbType.Int, GetPrimaryKeyValue(entity));
            AddPrimaryKeyToFilter(filter, entity);
            Delete(filter);
        }

        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="filter"/>
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public void Delete(DataFilter<T> filter)
        {            
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                ApplyDeleteToSqlCommander(dac, filter);
                dac.ExecNonQuery();                
            }
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
            var filter = new DataFilter<T>(Map);
            AddPrimaryKeyToFilter(filter, entity);
            await DeleteAsync(filter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="filter"/>
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task DeleteAsync(DataFilter<T> filter)
        {
            await DeleteAsync(filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes records from the database using a where clause defined by <paramref name="filter"/>
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(DataFilter<T> filter, CancellationToken cancellationToken)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                ApplyDeleteToSqlCommander(dac, filter);
                await dac.ExecNonQueryAsync(cancellationToken).ConfigureAwait(false);                
            }
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> without a return value.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <returns></returns>
        public void ExecuteNonQuery(string sqlText)
        {
            ExecuteScalar<int>(sqlText);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> without a return value.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <returns></returns>
        public async Task ExecuteNonQueryAsync(string sqlText)
        {
            await ExecuteScalarAsync<int>(sqlText).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> without a return value. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        public void ExecuteNonQuery(string sqlText, DataFilter<T> filter)
        {
            ExecuteScalar<int>(sqlText, filter);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> without a return value. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        public async Task ExecuteNonQueryAsync(string sqlText, DataFilter<T> filter)
        {
            await ExecuteNonQueryAsync(sqlText, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> without a return value. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        public async Task ExecuteNonQueryAsync(string sqlText, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            await ExecuteScalarAsync<int>(sqlText, filter, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(string sqlText)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                return ExecuteScalar<TScalar>(sqlText, dac);
            }
        }

        
        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>        
        /// <param name="filter"></param>
        /// <returns></returns>
        public TScalar ExecuteScalar<TScalar>(string sqlText, DataFilter<T> filter)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                CreateWhereClause(filter, dac);//this will set the parameters on the DAC

                return ExecuteScalar<TScalar>(sqlText, dac);
            }
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <returns></returns>
        public async Task<TScalar> ExecuteScalarAsync<TScalar>(string sqlText)
        {            
            return await ExecuteScalarAsync<TScalar>(sqlText, null).ConfigureAwait(false);            
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>        
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<TScalar> ExecuteScalarAsync<TScalar>(string sqlText, DataFilter<T> filter)
        {
            return await ExecuteScalarAsync<TScalar>(sqlText, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>        
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TScalar> ExecuteScalarAsync<TScalar>(string sqlText, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                if(filter != null)
                    CreateWhereClause(filter, dac);//this will set the parameters on the DAC
                dac.SqlText = sqlText;

                var result = await dac.ExecScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result != null && result is TScalar)
                    return (TScalar)result;
                return default(TScalar);
            }
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/> with a return value of <typeparamref name="TScalar"/>, using <paramref name="dac"/>.
        /// </summary>
        /// <param name="sqlText">The SQL text.</param>
        /// <param name="dac">custom SqlCommander that allow passing of parameters</param>
        /// <returns></returns>
        internal TScalar ExecuteScalar<TScalar>(string sqlText, SqlCommander dac)
        {
            object result;
            
            dac.SqlText = sqlText;
            
            result = dac.ExecScalar();
            if (result != null && result is TScalar)
                return (TScalar)result;
            return default(TScalar);            
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<Y> ExecuteSet<Y>(string sqlText)
        {
            return ExecuteSet<Y>(sqlText, null);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>        
        /// <returns></returns>
        public List<Y> ExecuteSet<Y>(string sqlText, DataFilter<T> filter)
        {
            var result = new List<Y>();

            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                dac.SqlText = sqlText;

                if (filter != null)
                    CreateWhereClause(filter, dac);//this will set the parameters on the DAC

                //call database
                using (SqlDataReader reader = dac.ExecReader())
                {
                    var type = typeof(Y);

                    //read the first result set
                    while (reader.Read())
                    {
                        //get the first column of each row and add its value to the result
                        if (reader.FieldCount > 0)
                        {
                            var value = reader.GetValue(0);

                            Y castedValue;
                            //convert to a value we can use
                            if (value == DBNull.Value)
                            {
                                castedValue = default(Y);
                            }
                            else
                            {                                
                                value = ReflectionHelper.ConvertValueToEnum(value, type);
                                castedValue = (Y)value;
                            }
                            
                            result.Add(castedValue);
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>        
        /// <returns></returns>
        public async Task<List<Y>> ExecuteSetAsync<Y>(string sqlText)
        {
            return await ExecuteSetAsync<Y>(sqlText, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        
        /// <returns></returns>
        public async Task<List<Y>> ExecuteSetAsync<Y>(string sqlText, DataFilter<T> filter)
        {
            return await ExecuteSetAsync<Y>(sqlText, filter, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a custom SQL statement defined by <paramref name="sqlText"/>. The first column of each row is added to the result. Parameters can be defined on <paramref name="filter"/>.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Y>> ExecuteSetAsync<Y>(string sqlText, DataFilter<T> filter, CancellationToken cancellationToken)
        {
            var result = new List<Y>();

            using (SqlCommander dac = new SqlCommander(ConnectionString, CommandTimeout))
            {
                dac.SqlText = sqlText;

                if (filter != null)
                    CreateWhereClause(filter, dac);//this will set the parameters on the DAC

                //call database
                using (SqlDataReader reader = await dac.ExecReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var type = typeof(Y);

                    //read the first result set
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        //get the first column of each row and add its value to the result
                        if (reader.FieldCount > 0)
                        {
                            var value = reader.GetValue(0);

                            Y castedValue;
                            //convert to a value we can use
                            if (value == DBNull.Value)
                            {
                                castedValue = default(Y);
                            }
                            else
                            {
                                value = ReflectionHelper.ConvertValueToEnum(value, type);
                                castedValue = (Y)value;
                            }

                            result.Add(castedValue);
                        }
                    }

                    return result;
                }
            }
        }


        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        public void BulkInsert(IEnumerable<T> entities)
        {
            BulkInsert(entities, false);
        }

        /// <summary>
        /// Inserts a collection of entities of <typeparamref name="T"/> using Sql Bulk Copy. The SqlDbType defined on the column attributes is ignored. Instead, the Sql Type is derived from the .NET type of the mapped properties.
        /// A list of supported types can be found here: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.datatype(v=vs.110).aspx
        /// This method supports System.Transaction.TransactionScope.
        /// Please mind that SqlBulkCopy is case sensitive with regards to column names.
        /// </summary>        
        /// <param name="entities"></param>
        /// <param name="identityInsert"></param>        
        public void BulkInsert(IEnumerable<T> entities, bool identityInsert)
        {
            BulkInsert(entities, identityInsert, SqlBulkCopyOptions.Default);
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
        public void BulkInsert(IEnumerable<T> entities, bool isIdentityInsert, SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            if (entities == null || entities.Count() == 0)
                return;

            var dataTable = Utility.CreateDataTableFromMap(Map, isIdentityInsert);            

            //create rows in the datatable for each entity
            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();
                foreach (var databaseColumn in Map.DatabaseColumns.Where(x=>x.IsReadOnly == false))
                {
                    //set values in the row for each column (and only if the column exists in the table definition)
                    if (dataTable.Columns.Contains(databaseColumn.Column))
                    {
                        var value = databaseColumn.Info.GetValue(entity);
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
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }                
    }
}
