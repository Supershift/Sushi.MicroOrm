using Sushi.MicroORM.Mapping;
using System.Reflection;
using static Sushi.MicroORM.FakeAssemblyTests.TestClass;
using static Sushi.MicroORM.FakeAssemblyTests.TestRecord;

namespace Sushi.MicroORM.FakeAssemblyTests
{
    public class DataMapScannerTest
    {

        [Fact]
        public void AssemblyScannerTest()
        {

            Assembly asm = typeof(TestMap).Assembly;

            System.Reflection.Assembly[] array = new System.Reflection.Assembly[1];
            array[0] = asm;

            var dataMapProvider = new DataMapProvider();
            var dataMapScanner = new DataMapScanner();

            dataMapScanner.Scan(array, dataMapProvider);
            var testClassResult = dataMapProvider.GetMapForType<TestClass>();
            var testRecordResult = dataMapProvider.GetMapForType<TestRecord>();

            Assert.NotNull(testClassResult);
            Assert.NotNull(testRecordResult);
            Assert.True(testClassResult is TestClassMap);
            Assert.True(testRecordResult is TestRecordMap);
        }

        private class TestMap : DataMap<TestClass> { }
    }
}
