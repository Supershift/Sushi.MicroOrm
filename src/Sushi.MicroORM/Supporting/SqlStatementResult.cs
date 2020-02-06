using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Supporting
{
    public class SqlStatementResult<T> where T: new()
    {
        public SqlStatementResult(SqlStatementResultType resultType)
        {
            ResultType = resultType;
            if (resultType == SqlStatementResultType.Multiple)
                MultipleResults = new List<T>();
        }
        public SqlStatementResultType ResultType { get; private set; }
        public T SingleResult { get; set; }
        public List<T> MultipleResults { get; }
    }
}
