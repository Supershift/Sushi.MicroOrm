﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.MicroORM.Tests.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests
{
    [TestClass]
    public class ConnectorTestAsync
    {
        Connector<Order> ConnectorOrders { get { return new Connector<Order>(); } }
        Connector<Product> ConnectorProducts { get { return new Connector<Product>(); } }

        [TestMethod]
        public async Task FetchSingleByIDAsync()
        {
            int id = 1;
            var order = await ConnectorOrders.FetchSingleAsync(id);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public async Task FetchSingleByFilterAsync()
        {
            int id = 2;

            var filter = ConnectorOrders.CreateDataFilter();
            filter.Add(x => x.ID, id);

            var order = await ConnectorOrders.FetchSingleAsync(filter);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.AreEqual(id, order?.ID);
        }

        [TestMethod]
        public async Task FetchSingleBySqlAsync()
        {
            int id = 3;

            string sql = $"SELECT * FROM cat_Orders WHERE Order_Key = {id}"; //this is BAD PRACTICE! always use parameters

            var order = await ConnectorOrders.FetchSingleAsync(sql);

            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public async Task FetchSingleBySqlAndFilterAsync()
        {
            int id = 3;

            string sql = $"SELECT * FROM cat_Orders WHERE Order_Key = @orderID"; //this is BAD PRACTICE! always use parameters

            var filter = ConnectorOrders.CreateDataFilter();
            filter.AddParameter("@orderID", id);

            var order = await ConnectorOrders.FetchSingleAsync(sql, filter);
            
            Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");

            Assert.IsTrue(order?.ID == id);
        }

        [TestMethod]
        public async Task FetchAllAsync()
        {
            var request = new DataFilter<Order>();
            var orders = await ConnectorOrders.FetchAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count > 0);
        }

        [TestMethod]
        public async Task TestCancellation()
        {
            var cts = new CancellationTokenSource();
            var filter = new DataFilter<Order>();
            cts.Cancel();
            try
            {
                var orders = await ConnectorOrders.FetchAllAsync(filter, cts.Token);
                Assert.Fail();
            }
            catch(TaskCanceledException)
            {
                
            }
            
            
        }

        [TestMethod]
        public async Task FetchAllInvalidMapAsync()
        {
            var connector = new Connector<Order>(new Order.InvalidOrderMap());
            var request = connector.CreateDataFilter();
            try
            {
                var orders = await connector.FetchAllAsync(request);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("no table"))
                    Assert.Fail();
            }
        }

        [TestMethod]
        public async Task FetchAllBySqlAsync()
        {
            string sql = "SELECT TOP(3) * FROM cat_Orders";
            var orders = await ConnectorOrders.FetchAllAsync(sql);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count > 0);
        }

        [TestMethod]
        public async Task FetchPagingAsync()
        {
            var request = new DataFilter<Order>();
            request.Paging = new PagingData()
            {
                NumberOfRows = 5,
                PageIndex = 2
            };
            var orders = await ConnectorOrders.FetchAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }
            Console.WriteLine("Total number of rows: " + request.Paging.TotalNumberOfRows);

            Assert.IsTrue(orders.Count == request.Paging.NumberOfRows);
            Assert.IsTrue(request.Paging.TotalNumberOfRows.HasValue);
        }

        [TestMethod]
        public async Task FetchWhereInIntAsync()
        {

            var request = new DataFilter<Order>();
            //request.WhereClause.Add(new DatabaseDataValueColumn("Order_Key", System.Data.SqlDbType.Int, new int[] { 1, 2,3 }, ComparisonOperator.In));
            request.Add(x => x.ID, new int[] { 1, 2, 3 }, ComparisonOperator.In);
            var orders = await ConnectorOrders.FetchAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.IsTrue(orders.Count == 3);
        }

        [TestMethod]
        public async Task FetchWhereInEmptyEnumerableAsync()
        {
            var request = new DataFilter<Order>();
            
            request.Add(x => x.ID, new int[] { }, ComparisonOperator.In);
            var orders = await ConnectorOrders.FetchAllAsync(request);
            foreach (var order in orders)
            {
                Console.WriteLine($"{order.ID} - {order.Created} - {order.CustomerID}");
            }

            Assert.AreEqual(0, orders.Count);
        }

        [TestMethod]
        public async Task FetchWhereInStringAsync()
        {
            var request = new DataFilter<Product>();
            var names = new string[]
            {
                "TV",
                "Laser Mouse",
                "E-reader"
            };

            request.Add(x => x.Name, names, ComparisonOperator.In);
            var products = await ConnectorProducts.FetchAllAsync(request);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.Name}");
            }

            Assert.IsTrue(products.Count == 3);
        }

        [TestMethod]
        public async Task FetchWhereGreaterThanStringAsync()
        {
            var filter = ConnectorProducts.CreateDataFilter();

            filter.Add(x => x.Description, "", ComparisonOperator.GreaterThan);
            var products = await ConnectorProducts.FetchAllAsync(filter);
            foreach (var product in products)
            {
                Console.WriteLine($"{product.ID} - {product.Name}");
            }

            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public async Task FetchNotExistingAsync()
        {
            var order = await ConnectorOrders.FetchSingleAsync(-1);
            Assert.IsTrue(order == null);
        }
        [TestMethod]
        public async Task ExecuteNonQueryAsync()
        {
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var query = @"
UPDATE cat_Products
SET Product_Name = @name
WHERE Product_Key = @productID";
            var filter = new DataFilter<Product>();
            filter.AddParameter(@"name", System.Data.SqlDbType.VarChar, name);
            filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            await ConnectorProducts.ExecuteNonQueryAsync(query, filter);

            //check if name was updated
            var product = await ConnectorProducts.FetchSingleAsync(productID);
            Assert.AreEqual(name, product.Name);
        }

        [TestMethod]
        public async Task ExecuteScalarAsync()
        {
            string name = DateTime.UtcNow.Ticks.ToString();
            int productID = 1;
            var query = @"
SELECT COUNT(*)
FROM cat_Products
WHERE Product_Key = @productID";
            var filter = new DataFilter<Product>();
            filter.AddParameter("@productID", System.Data.SqlDbType.Int, productID);
            var count = await ConnectorProducts.ExecuteScalarAsync<int>(query, filter);
            Assert.AreEqual(1, count);

        }

        [TestMethod]
        public async Task SaveNewAsync()
        {
            var order = new Order()
            {
                CustomerID = 1,
                Created = DateTime.UtcNow,
                DeliveryTime = new TimeSpan(3, 20, 35),
                DeliveryTime2 = DateTime.UtcNow.TimeOfDay
            };
            await ConnectorOrders.SaveAsync(order);

            Assert.IsTrue(order.ID > 0);
        }

        [TestMethod]
        public async Task SaveExistingAsync()
        {
            //save the order
            var order = ConnectorOrders.FetchSingle(20);


            string newComments = DateTime.UtcNow.ToString();

            order.Comments = newComments;
            order.DeliveryTime = null;
            order.DeliveryTime2 = DateTime.UtcNow.TimeOfDay;
            ConnectorOrders.Save(order);

            //retrieve order again from database
            order = await ConnectorOrders.FetchSingleAsync(20);


            Assert.AreEqual(newComments, order.Comments);
        }

        [TestMethod]
        public async Task FetchWithTableValuedParameterAsync()
        {
            var sproc = "EXEC sp_GetOrders @customerIDs";

            var customerTable = new System.Data.DataTable();
            customerTable.Columns.Add();
            customerTable.Rows.Add(98);
            customerTable.Rows.Add(99);

            var connector = new Connector<Order>();
            var filter = connector.CreateDataFilter();
            filter.AddParameter("@customerIDs", customerTable, "cat_CustomerTableType");
            var orders = await connector.FetchAllAsync(sproc, filter);

            var count98 = orders.Count(x => x.CustomerID == 98);
            var count99 = orders.Count(x => x.CustomerID == 99);

            Assert.AreEqual(orders.Count, count98 + count99);
        }

        [TestMethod]
        public async Task InsertAsync()
        {
            var product = new Product()
            {
                Description = "New insert test",
                Name = "New insert",
                ExternalID = null,
                Price = 12.50M,
                BarCode = Encoding.UTF8.GetBytes("SKU-12345678")
            };
            await ConnectorProducts.InsertAsync(product);
        }

        [TestMethod]
        public async Task InsertComposityKeyAsync()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = Guid.NewGuid().GetHashCode(),
                SecondID = Guid.NewGuid().GetHashCode(),
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = new Connector<CompositeKey>();
            await connector.InsertAsync(compositeKey);
        }

        [TestMethod]
        public async Task UpdateComposityKey()
        {
            var compositeKey = new CompositeKey()
            {
                FirstID = 1,
                SecondID = 1,
                SomeValue = Guid.NewGuid().ToString()
            };
            var connector = new Connector<CompositeKey>();
            await connector.UpdateAsync(compositeKey);

            var filter = connector.CreateDataFilter();
            filter.Add(x => x.FirstID, 1);
            filter.Add(x => x.SecondID, 1);
            var result = await connector.FetchSingleAsync(filter);

            Assert.AreEqual(compositeKey.SomeValue, result.SomeValue);
        }
    }
}