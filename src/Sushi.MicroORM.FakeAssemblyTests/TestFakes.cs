using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.FakeAssemblyTests
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

    public record class TestRecord
    {
        public class TestRecordMap : DataMap<TestRecord>
        {
            public TestRecordMap()
            {
                Map(x => x.MutableValue, "Value");
            }
        }        
        
        public int MutableValue { get; set; }
    }

    public enum TestEnum
    {
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4
    }
}
