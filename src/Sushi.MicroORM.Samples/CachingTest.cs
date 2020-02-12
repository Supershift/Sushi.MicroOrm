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
        public void FetchSingleBySql()
        {
            int id = 1;
            string query = "SELECT * FROM cat_Orders WHERE Order_Key = @orderID";

            var connector = new Caching.CachedConnector<Order>();
            var filter = connector.CreateDataFilter();
            filter.AddParameter("@orderID", id);
            //get first instance (will be server by the database)
            var result = connector.FetchSingle(query, filter);

            //get it again
            var cachedResult = connector.FetchSingle(query, filter);

            //if the second call was served from cache, it will have reference equality
            Assert.AreEqual(result, cachedResult);
        }

        [TestMethod]
        public void FetchAll()
        {            
            var connector = new Caching.CachedConnector<Order>();
            var filter = connector.CreateDataFilter();
            //get first instance (will be server by the database)
            var result = connector.FetchAll(filter);

            //get it again
            var cachedResult = connector.FetchAll(filter);

            //if the second call was served from cache, it will have reference equality
            Assert.AreEqual(result, cachedResult);
        }

        [TestMethod]
        public void FetchAllBySql()
        {            
            string query = "SELECT * FROM cat_Orders";

            var connector = new Caching.CachedConnector<Order>();
            
            //get first instance (will be server by the database)
            var result = connector.FetchAll(query);

            //get it again
            var cachedResult = connector.FetchAll(query);

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

            //delete an instance 
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
