using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.FakeAssembly
{
    public class TestClass
    {
        public class TestClassMap : DataMap<TestClass>
        {
            public TestClassMap()
            {
                Id(x => x.Id, "ID");
                Map(x => x.Name, "Name");
            }
        }
        
        public int Id { get; set; }
        public string? Name { get; set; }
    }


    public class TestClassMap2 : DataMap<TestRecord>
    {
        
    }

    public record TestRecord
    {

    }

    internal class InternalMap : DataMap<InternalClass>
    {

    }

    internal class InternalClass
    {

    }
}
