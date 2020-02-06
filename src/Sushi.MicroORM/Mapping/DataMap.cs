﻿using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Sushi.MicroORM.Mapping.DataMap;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Represents the mapping between database objects and code objects.
    /// </summary>
    public class DataMap 
    {   
        /// <summary>
        /// The callback that is triggered after the SQL DELETE statement has been executed.
        /// </summary>
        public AfterSaveHandler OnPostDelete { get; set; }
        internal void DoPostDelete(DataMap map)
        {
            if (OnPostDelete != null) OnPostDelete(map);
        }

        /// <summary>
        /// The callback that is triggered after the SQL INSERT or UPDATE statement has been executed.
        /// </summary>
        public AfterSaveHandler OnPostSave { get; set; }
        internal void DoPostSave(DataMap map)
        {
            if (OnPostSave != null) OnPostSave(map);
        }

        /// <summary>
        /// The callback that is triggered before the SQL statement is created allowing for applying change to the statement.
        /// </summary>
        public AfterFetchHandler OnPostFetch { get; set; }
        internal void DoPostFetch(DataMap map, Query query)
        {
            if (OnPostFetch != null) OnPostFetch(new QueryData() { Map = map, Query = query });
        }
        /// <summary>
        /// The callback that is triggered before the SQL statement is created allowing for applying change to the statement.
        /// </summary>
        public BeforeFetchHandler OnBeforeFetch { get; set; }
        internal void DoBeforeFetch(DataMap map, Query query)
        {
            if (OnBeforeFetch != null) OnBeforeFetch(new QueryData() { Map = map, Query = query });
        }

        

        /// <summary>
        /// Initializes a new instance of <see cref="DataMap"/> for a type defined by <paramref name="mappedType"/>.
        /// </summary>
        /// <param name="mappedType"></param>
        public DataMap(Type mappedType)
        {
            if (mappedType == null)
                throw new ArgumentNullException(nameof(mappedType));

            MappedType = mappedType;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the class for which this <see cref="DataMap"/> defines a mapping.
        /// </summary>
        protected Type MappedType { get; set; }

        /// <summary>
        /// Gets the name of the table in the database to which class T is mapped
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// Sets the name of the table in the database to which class T is mapped
        /// </summary>
        /// <param name="tableName"></param>
        public TableSetter Table(string tableName)
        {
            TableName = tableName;
            return new TableSetter() { Map = this };
        }

        /// <summary>
        /// Gets a collection of <see cref="DataMapItem"/> objects that define the mapping between object and database.
        /// </summary>
        public List<DataMapItem> Items { get; } = new List<DataMapItem>();

        /// <summary>
        /// Gets all columns that are mapped to a primary key column.
        /// </summary>
        /// <returns></returns>
        public List<DataMapItem> GetPrimaryKeyColumns()
        {
            return Items.Where(x => x.IsPrimaryKey).ToList();
        }        

        /// <summary>
        /// Validates if this map can be used to generate queries.
        /// </summary>
        internal void ValidateMappingForGeneratedQueries()
        {
            if (string.IsNullOrWhiteSpace(TableName))
                throw new Exception("This mapping cannot be used to generate queries because no tablename is defined. Use Map.Table() to specify a tablename.");
        }

        
    }

    /// <summary>
    /// Represents the mapping between database objects and code objects.
    /// </summary>
    /// <typeparam name="T">Class to map with SQL table and columns.</typeparam>
    public class DataMap<T> : DataMap where T : new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataMap{T}"/>.
        /// </summary>
        public DataMap() : base(typeof(T))
        {
            
        }        

        /// <summary>
        /// Maps the property defined by <paramref name="memberExpression"/> to <paramref name="columnName"/> as (part of) a primary key. By default, the property will be mapped as an 
        /// identity column.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="columnName"></param>        
        /// <returns></returns>
        public DataMapItemSetter Id(Expression<Func<T, object>> memberExpression, string columnName)
        {
            DataMapItem dbcol = new DataMapItem
            {
                Sender = this,
                Column = columnName,
                IsPrimaryKey = true,
                IsIdentity = true                
            };

            var members = ReflectionHelper.GetMemberTree(memberExpression);
            dbcol.MemberInfoTree.AddRange(members);

            var memberType = ReflectionHelper.GetMemberType(dbcol.MemberInfoTree);
            dbcol.SqlType = Utility.GetSqlDbType(memberType);
            Items.Add(dbcol);

            return new DataMapItemSetter(dbcol);
        }

        /// <summary>
        /// Maps the property defined by <paramref name="memberExpression"/> to <paramref name="columnName"/> as (part of) a primary key.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public DataMapItemSetter Map(Expression<Func<T, object>> memberExpression, string columnName)
        { 
            DataMapItem dbcol = new DataMapItem
            {
                Sender = this,
                Column = columnName                
            };

            var members = ReflectionHelper.GetMemberTree(memberExpression);
            dbcol.MemberInfoTree.AddRange(members);

            var memberType = ReflectionHelper.GetMemberType(dbcol.MemberInfoTree);
            dbcol.SqlType = Utility.GetSqlDbType(memberType);
            Items.Add(dbcol);

            return new DataMapItemSetter(dbcol);
        }
    }
}
