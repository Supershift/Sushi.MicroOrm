namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Specifies options for the DML Clause of a SQL statement.
    /// </summary>
    public enum DMLStatementType
    {
        /// <summary>
        /// The statement selects rows from the database.
        /// </summary>
        Select,

        /// <summary>
        /// The statement updates rows in the database.
        /// </summary>
        Update,

        /// <summary>
        /// The statement inserts rows into the database.
        /// </summary>
        Insert,

        /// <summary>
        /// The statement deletes rows from the database.
        /// </summary>
        Delete,

        /// <summary>
        /// The statement inserts or updates rows in the database.
        /// </summary>
        InsertOrUpdate,

        /// <summary>
        /// The statement can contain anything.
        /// </summary>
        CustomQuery
    }
}