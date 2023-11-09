namespace Sushi.MicroORM
{
    /// <summary>
    /// A set of connection strings for a Sql Azure database.
    /// </summary>
    public record SqlServerConnectionString : ConnectionString
    {
        /// <summary>
        /// Creates a new instance of <see cref="SqlServerConnectionString"/>.
        /// </summary>        
        /// <param name="primary"></param>
        /// <param name="generateReadOnly">If set to true, adds ApplicationIntent=ReadOnly to the connection string</param>
        public SqlServerConnectionString(string primary, bool generateReadOnly) : base(primary, generateReadOnly ? GenerateReadOnly(primary) : null)
        {
            
        }

        /// <summary>
        /// Creates a new instance of <see cref="SqlServerConnectionString"/>.
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="readOnly"></param>
        public SqlServerConnectionString(string primary, string? readOnly) : base(primary, readOnly)
        {
            
        }


        private static string GenerateReadOnly(string connectionString) 
        {
            if (!connectionString.EndsWith(';')) connectionString += ';';
            connectionString  += "ApplicationIntent=ReadOnly;";
            return connectionString;
        }
    }
}
