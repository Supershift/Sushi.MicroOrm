using Sushi.MicroORM.Mapping;
using System.Reflection;
using static Sushi.MicroORM.FakeAssemblyTests.TestClass;

namespace Sushi.MicroORM.FakeAssemblyTests
{
    public class DataMapScannerTest
    {

        [Fact]
        public void AssemblyScannerTest()
        {

            Assembly asm = typeof(TestClass).Assembly;

            System.Reflection.Assembly[] array = new System.Reflection.Assembly[1];
            array[0] = asm;

            var dataMapProvider = new DataMapProvider();
            var dataMapScanner = new DataMapScanner();

            dataMapScanner.Scan(array, dataMapProvider);
            var testClassResult = dataMapProvider.GetMapForType<TestClass>();
            var testRecordResult = dataMapProvider.GetMapForType<TestRecord>();

            Assert.NotNull(testClassResult);
            Assert.NotNull(testRecordResult);
            Assert.True(testClassResult is DataMap<TestClass>);
            Assert.True(testRecordResult is DataMap<TestRecord>);
        }
    }
}
