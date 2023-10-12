using Sushi.MicroORM.Mapping;
using System.Reflection;

namespace Sushi.MicroORM.UnitTests
{
    public class DataMapScannerTest
    {

        [Fact]
        public void AssemblyScannerTest()
        {
            Assembly asm = typeof(FakeAssembly.TestClass).Assembly;

            Assembly[] array = new Assembly[1];
            array[0] = asm;

            var dataMapProvider = new DataMapProvider();
            var dataMapScanner = new DataMapScanner();

            dataMapScanner.Scan(array, dataMapProvider);
            var testClassResult = dataMapProvider.GetMapForType<FakeAssembly.TestClass>();
            var testRecordResult = dataMapProvider.GetMapForType<FakeAssembly.TestRecord>();
            var internalClassResult = dataMapProvider.GetMapForType<FakeAssembly.InternalClass>();

            Assert.NotNull(testClassResult);
            Assert.NotNull(testRecordResult);
            Assert.True(testClassResult is DataMap<FakeAssembly.TestClass>);
            Assert.True(testRecordResult is DataMap<FakeAssembly.TestRecord>);
        }
    }
}
