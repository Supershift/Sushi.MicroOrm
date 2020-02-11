using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.MicroORM.Samples.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Samples
{
    [TestClass]
    public class CachingTest
    {
        //running both tests below at the same time can cause unexpected results
        [TestMethod]
        public void FetchSingle()
        {
            int id = 1;
            var connector = new Caching.CachedConnector<Order>();
            //get first instance (will be server by the database)
            var result = connector.FetchSingle(id);

            //get it again
            var cachedResult = connector.FetchSingle(id);

            //if the second call was served from cache, it will have reference equality
            Assert.AreEqual(result, cachedResult);
        }

        [TestMethod]
        public void FlushCache()
        {
            int id = 1;
            var connector = new Caching.CachedConnector<Order>();
            //get first instance (will be served by the database)
            var result = connector.FetchSingle(id);

            //delete some instance 
            var deleteFilter = connector.CreateDataFilter();
            deleteFilter.Add(x => x.ID, -1);
            connector.Delete(deleteFilter);

            //get it again
            var cachedResult = connector.FetchSingle(id);

            //if the second call was served from cache, it will have reference equality
            Assert.AreNotEqual(result, cachedResult);
        }
    }
}
