using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// The entity that contains information related to the sql statement.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// The Ctor, this initiates the select list.
        /// </summary>
        public Query()
        {
            this.Select = new List<DataMapItem>();
        }
        /// <summary>
        /// Gets or set the select columns that are part of the sql statement.
        /// </summary>
        public List<DataMapItem> Select { get; set; }
        /// <summary>
        /// Gets or set the from clause from the sql statement.
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// Gets or set the where clause from the sql statement.
        /// </summary>
        public string Where { get; set; }
        /// <summary>
        /// Gets or set the order by clause from the sql statement.
        /// </summary>
        public string OrderBy { get; set; }
        /// <summary>
        /// Get the sql statement that is about to be fired.
        /// </summary>
        internal string Sql { get; set; }
        internal string ParameterInfo { get; set; }

        public string UniqueIdentifier
        {
            get
            {
                return $"{From} {ParameterInfo}";
            }
        }

        public object Result { get; set; }
    }
}