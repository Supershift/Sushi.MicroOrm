using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Mapping
{
    public delegate void QueryHandler(QueryData data);
    public delegate void QueryResultHandler(QueryDataOutput data);
    public delegate void BeforeFetchHandler(QueryData data);
    public delegate void AfterFetchHandler(QueryData data);
}
