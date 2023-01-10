using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.UnitTests
{
    public class SqlStatementGeneratorTest
    {
        [Fact]
        public void SelectSingleRowTest()
        {
            var generator = new SqlStatementGenerator();
            var map = new MyMap();
            var query = new DataQuery<MyClass>(map);
            query.Add(x => x.Id, 1);

            var statement = generator.GenerateSqlStatment(DMLStatementType.Select, SqlStatementResultCardinality.SingleRow, map, query);

            var sql = statement.ToString();
        }

        public class MyClass
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class MyMap : DataMap<MyClass>
        {
            public MyMap()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name");
            }
        }

        public class MyMapWithAlias : DataMap<MyClass>
        {
            public MyMapWithAlias()
            {
                Table("MyTable");
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name").Alias("FullName");
            }
        }
    }
}
