namespace Sushi.MicroORM
{
    /// <summary>
    /// Defines operators that can be used to combine conditions in a where clause
    /// </summary>
    public enum WhereConditionOperator
    {
        /// <summary>
        /// Logical AND operator
        /// </summary>
        And,

        /// <summary>
        /// Using Or will combine the previous and the current conditions in an OR group (A = 1 or B = 2)
        /// </summary>
        Or,

        /// <summary>
        ///  Using OrUngrouped will just add the or statement without setting an or group
        /// </summary>
        OrUngrouped
    }
}