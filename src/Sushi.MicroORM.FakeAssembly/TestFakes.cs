using Sushi.MicroORM.Mapping;

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

    internal abstract class BaseMap<T> : DataMap<T>
    {

    }

    internal class GenericMap<T> : DataMap<object>
    {

    }
}
